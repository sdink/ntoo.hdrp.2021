using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using SimpleJSON;

public class LiveBase : MonoBehaviour
{
    public string LiveServerHostIP = "localhost";
    public int LiveServerHostPort = 802;
    public bool ConnectOnPlay = true;
    public bool ReconnectOnLostConnection = true;
    public bool DropPackets = true;
    public LiveCharacterSetupFile ExpressionSetFile;
    //Connection to Live Server
    public LiveConnection live;
    //Character Setup that Plugin is Driving
    public LiveCharacterSetup character = new LiveCharacterSetup();
    // A handle to the current DataSet driving the rig
    public SimpleJSON.JSONNode currentDatSet;
    // Threshold of how many calls to get data resulting in no data before we consider connection to Live Server lost.
    private int TimeoutThreshold = 90; // Roughly 1 and 1/2 seconds
    private int m_EmptyPacketsCounter = 0;

    //Recording specific
    public bool EnableRecording = false;
    public bool RecordOnStart = false;
    public AnimationClip ClipToWriteTo;
    public bool Recording = false;
    public bool KeyframeOnNewData = false;
    protected Dictionary<string, Vector4> cachedRigValues;
#if UNITY_EDITOR && (UNITY_5 || UNITY_2017 || UNITY_2018)
    #if UNITY_5_4_OR_NEWER
        public UnityEditor.AnimationUtility.TangentMode keyframeTangentMode = UnityEditor.AnimationUtility.TangentMode.Constant;
    #endif
#endif


    class FTIAnimCurve
    {
        public FTIAnimCurve()
        {
            animCurves = new List<AnimationCurve>();
            isBlendShape = false;
            hierachyPath = "";
        }
    
        public string hierachyPath;
        public bool isBlendShape;
        public List<AnimationCurve> animCurves;
    }
    Dictionary<string, FTIAnimCurve> RecordedCurves;
    float recordingTime;

    protected virtual void Start()
    {
        //Load the Character 
        if (ExpressionSetFile != null)
        {
            character.Init(ExpressionSetFile.Expressions, ExpressionSetFile.Controls);
            List<string> missingCtrlList = LiveUnityInterface.ValidateControls(this.gameObject, character.GetControlList());
            if (missingCtrlList.Count > 0)
            {
                string msg = "[Faceware Live] These controls are not in your scene:\n";
                foreach (string ctrl in missingCtrlList)
                {
                    msg += ctrl;
                    msg += "\n";
                }
                Debug.LogWarning(msg);
            }

            //Setup Connection to Live
            live = new LiveConnection(LiveServerHostIP, LiveServerHostPort);
            live.m_Reconnect = ReconnectOnLostConnection;
            // Connect on Play if toggled true
            if (ConnectOnPlay)
                Connect();
            live.m_DropPackets = DropPackets;
        }

        if (RecordOnStart && EnableRecording)
            OnToggleRecording();
    }

    private void OnDestroy()
    {
        if (Recording)
            OnToggleRecording();
    }

    public void BaseUpdate()
    {        
        if (live != null && live.IsConnected() && ExpressionSetFile)
        {
            if (currentDatSet != null && currentDatSet.Count > 0)
            {
                m_EmptyPacketsCounter = 0;
            }
            else if (m_EmptyPacketsCounter >= TimeoutThreshold)
            {
                // Force reconnect as threshold of no data recieved has been met.
                Disconnect();
                Connect();
                m_EmptyPacketsCounter = 0;
            }
            else
            {
                m_EmptyPacketsCounter++;
            }
        }
    }

    public void LateUpdate()
    {
        if (Recording)
        {
            recordingTime += Time.deltaTime;

            RecordFrame();
        }
    }

    // API to call a specific dataset from the current json object driving the rig
    public SimpleJSON.JSONNode GetControl(string controlName)
    {
        SimpleJSON.JSONNode control = null;
        try
        {
            control = currentDatSet[controlName];
        }
        catch
        {
            Debug.Log("[Faceware Live] Requested object is not within that data object. Ensure Live is streaming the requested data...");
        }
        return control;
    }

    public void Connect()
    {
        if (live != null)
        {
            if (!live.IsConnected())
            {
                live.m_HostIP = LiveServerHostIP;
                live.m_HostPort = LiveServerHostPort;
                live.Connect();
            }
            else
            {
                Debug.LogWarning("[Faceware Live] Live Client is already connected!");
            }
        }
        else
            Debug.LogWarning("[Faceware Live] Unable to Connect to Live Server Due to bad configurations or scene is not playing...");
    }

    public void Disconnect()
    {
        if (live != null)
            live.Disconnect();
    }

    public void OnSettingsChange()
    {
        if (live != null)
        {
            live.m_Reconnect = ReconnectOnLostConnection;
            live.m_DropPackets = DropPackets;
        }
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    // Note: Calibration can only be triggered if LiveServer and Unity are both on the same machine.
    // Calibration Logic utilizes Windows APIs to send a message directly to Live Server.
    public void CalibrateLiveServer()
    {
        IEnumerable<IntPtr> windows = LiveHelpers.FindWindowsWithText("Faceware Live");

        LiveHelpers.COPYDATASTRUCT cds = new LiveHelpers.COPYDATASTRUCT(); // Live only utilizes the dwData at this time
        cds.dwData = (IntPtr)1;

        int liveServerCount = 0;
        foreach (var window in windows)
        {
            LiveHelpers.SendMessage(window, 0X004A, IntPtr.Zero, ref cds);
            liveServerCount++;
        }

        if (liveServerCount > 0)
        {
            Debug.Log("[Faceware Live] Calibration message sent to " + liveServerCount + " instance(s) of Live!");
        }
        else
        {
            Debug.Log("[Faceware Live] No local instance of Live Server found! Please start Live Server!");
        }
    }
#endif

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void OnToggleRecording()
    {
        if (RecordedCurves == null)
        {
            RecordedCurves = new Dictionary<string, FTIAnimCurve>();
        }

        Recording = !Recording;

        if (!Recording)
        {
            //Write data to clip
            Debug.Log("[Faceware Live] Recording Stopped. Writing to animation clip. This may take a moment.");
            if (ClipToWriteTo != null)
            {
                foreach (KeyValuePair<string, FTIAnimCurve> curveKvp in RecordedCurves)
                {
                    string controlName = curveKvp.Key;
                    FTIAnimCurve curveData = curveKvp.Value;

                    if (curveData.isBlendShape)
                    {
#if UNITY_EDITOR && (UNITY_5 || UNITY_2017 || UNITY_2018)
    #if UNITY_5_4_OR_NEWER
                        //If in editor and a version that supports curve setting, set the curves to what is specified
                        for (int i = 0; i < curveData.animCurves[0].keys.Length; i++)
                        {
                            UnityEditor.AnimationUtility.SetKeyLeftTangentMode(curveData.animCurves[0], i, keyframeTangentMode);
                            UnityEditor.AnimationUtility.SetKeyRightTangentMode(curveData.animCurves[0], i, keyframeTangentMode);
                        }
    #endif
#endif
                        string blendShapeAttr = LiveHelpers.GetAttrString(controlName);
                        ClipToWriteTo.SetCurve(curveData.hierachyPath, typeof(SkinnedMeshRenderer), "blendShape." + blendShapeAttr, curveData.animCurves[0]);
                    }
                    else
                    {
                        string controlAttr = LiveHelpers.GetAttrString(controlName);

                        if (controlAttr == LiveCharacterSetup.rotationSuffix)
                        {
                            ClipToWriteTo.SetCurve(curveData.hierachyPath, typeof(Transform), "m_LocalRotation.x", curveData.animCurves[0]);
                            ClipToWriteTo.SetCurve(curveData.hierachyPath, typeof(Transform), "m_LocalRotation.y", curveData.animCurves[1]);
                            ClipToWriteTo.SetCurve(curveData.hierachyPath, typeof(Transform), "m_LocalRotation.z", curveData.animCurves[2]);
                            ClipToWriteTo.SetCurve(curveData.hierachyPath, typeof(Transform), "m_LocalRotation.w", curveData.animCurves[3]);
                        }
                        else if (controlAttr == LiveCharacterSetup.translationSuffix)
                        {
                            ClipToWriteTo.SetCurve(curveData.hierachyPath, typeof(Transform), "m_LocalPosition.x", curveData.animCurves[0]);
                            ClipToWriteTo.SetCurve(curveData.hierachyPath, typeof(Transform), "m_LocalPosition.y", curveData.animCurves[1]);
                            ClipToWriteTo.SetCurve(curveData.hierachyPath, typeof(Transform), "m_LocalPosition.z", curveData.animCurves[2]);
                        }
                    }
                }
                Debug.Log("[Faceware Live] Animation successfully written to " + ClipToWriteTo.name + ".anim");
                RecordedCurves.Clear();
            }
            else
            {
                Debug.LogWarning("[Faceware Live] Animtion could not be recorded as Animation Clip to write to was null. Please set an animation clip to write the animation to");
            }
        }
        else
        {
            RecordedCurves.Clear();
            IntializeRecording();
            Debug.Log("[Faceware Live] Recording Started!");
        }
    }

    void IntializeRecording()
    {
        List<string> controlNames = character.GetControlList();
        List<UnityEngine.Object> controls = LiveUnityInterface.GetControls(this.gameObject, controlNames);

        recordingTime = 0.0f;

        //Loop through controls and create anim curves for each
        for (int i = 0; i < controlNames.Count; i++)
        {
            FTIAnimCurve ftiCurve = new FTIAnimCurve();

            UnityEngine.Object control = controls[i];

            if (control is Transform)
            {
                Transform transfromControl = (Transform)control;

                ftiCurve.hierachyPath = LiveUnityInterface.GetRelativeGameObjectPath(transfromControl.gameObject, this.gameObject);
                ftiCurve.isBlendShape = false;

                int curveNum = 3;

                string attr = LiveHelpers.GetAttrString(controlNames [i]);

                if (attr == LiveCharacterSetup.rotationSuffix)
                    curveNum++;

                for (int j = 0; j < curveNum; j++)
                {
                    ftiCurve.animCurves.Add(new AnimationCurve());

                    //Intialize values to have at least 1 keyframe
                    if(attr != LiveCharacterSetup.rotationSuffix)
                    {
                        ftiCurve.animCurves[j].AddKey(0f, transfromControl.localPosition[j]);
                    }
                    else
                    {
                        ftiCurve.animCurves[j].AddKey(0f, transfromControl.localRotation[j]);
                    }
                }
            }
            else if (control is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer blendShapeControl = (SkinnedMeshRenderer)control;
                string attr = LiveHelpers.GetAttrString(controlNames[i]);
                int index = LiveUnityInterface.GetBlendShapeIndex(blendShapeControl, attr);

                ftiCurve.hierachyPath = LiveUnityInterface.GetRelativeGameObjectPath(blendShapeControl.gameObject, this.gameObject);
                ftiCurve.isBlendShape = true;
                ftiCurve.animCurves.Add(new AnimationCurve());
                ftiCurve.animCurves[0].AddKey(0f, blendShapeControl.GetBlendShapeWeight(index));
            }

            RecordedCurves.Add(controlNames[i], ftiCurve);
        }
    }

    public void RecordFrame()
    {
        if (Recording && live.m_RecievedNewData)
        {
            live.m_RecievedNewData = false;
            //Grab values from rig
            Dictionary<string, Vector4> controlValues;
            if (cachedRigValues == null)
            {
                controlValues = LiveUnityInterface.GetControlValues(this.gameObject, character.GetControlList());
            }
            else
            {
                controlValues = cachedRigValues;
            }

            //Write keyframes for each control, if keyframe on new data only check if there is new data
            foreach (KeyValuePair<string, Vector4> controlKvp in controlValues)
            {
                string controlName = controlKvp.Key;
                FTIAnimCurve curveData = RecordedCurves[controlName];

                if (curveData.isBlendShape)
                {
                    if(!KeyframeOnNewData || curveData.animCurves[0].keys[curveData.animCurves[0].keys.Length -1].value != controlKvp.Value.x)
                    {
                        Keyframe newFrame = new Keyframe(recordingTime, controlKvp.Value.x);
                        curveData.animCurves[0].AddKey(newFrame);
                    }
                }
                else
                {
                    if (!KeyframeOnNewData || curveData.animCurves[0].keys[curveData.animCurves[0].keys.Length - 1].value != controlKvp.Value.x)
                    {
                        Keyframe newFrame = new Keyframe(recordingTime, controlKvp.Value.x);
                        curveData.animCurves[0].AddKey(newFrame);
                    }

                    if (!KeyframeOnNewData || curveData.animCurves[1].keys[curveData.animCurves[1].keys.Length - 1].value != controlKvp.Value.y)
                    {
                        Keyframe newFrame = new Keyframe(recordingTime, controlKvp.Value.y);
                        curveData.animCurves[1].AddKey(newFrame);
                    }

                    if (!KeyframeOnNewData || curveData.animCurves[2].keys[curveData.animCurves[2].keys.Length - 1].value != controlKvp.Value.z)
                    {
                        Keyframe newFrame = new Keyframe(recordingTime, controlKvp.Value.z);
                        curveData.animCurves[2].AddKey(newFrame);
                    }

                    string attr = LiveHelpers.GetAttrString(controlName);

                    if (attr == LiveCharacterSetup.rotationSuffix)
                    {
                        if (!KeyframeOnNewData || curveData.animCurves[3].keys[curveData.animCurves[3].keys.Length - 1].value != controlKvp.Value.w)
                        {
                            Keyframe newFrame = new Keyframe(recordingTime, controlKvp.Value.w);
                            curveData.animCurves[3].AddKey(newFrame);
                        }
                    }
                }
            }
        }
    }
}