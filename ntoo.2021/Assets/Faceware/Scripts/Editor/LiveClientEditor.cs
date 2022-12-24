using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;

[CustomEditor(typeof(LiveClient))]
public class LiveClientEditor : Editor
{

  LiveClient FwLive ;
  Texture titleIcon ;
  GUIStyle titleStyle ;
  GUIStyle warningStyle;

  void OnEnable ()
  {
    titleStyle = new GUIStyle () ;
    titleStyle.fontStyle = FontStyle.Bold ;
    titleStyle.fontSize = 12 ;
    titleStyle.margin = new RectOffset (5, 0, 6, 6);

    FwLive = (LiveClient)target ;

    // Faceware Icon
    string iconPath = Utility.CombinePath( "Assets", "Faceware", "Scripts", "Editor", "Icons" );
    titleIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "LiveClient_DeviceHeader.png" ), typeof( Texture ) );

        warningStyle = new GUIStyle();
        warningStyle.fontStyle = FontStyle.Bold;
        warningStyle.normal.textColor = Color.red;
        warningStyle.wordWrap = true;
        warningStyle.alignment = TextAnchor.MiddleCenter;
    }

  public override void OnInspectorGUI()
  {
    EditorGUILayout.BeginVertical();
    {
        GUILayout.Space(15);
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(titleIcon, GUILayout.MinWidth(1));
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("Version " + LiveHelpers.CurrentVersion(), titleStyle);
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();

        GUILayout.Label( "Faceware Live Client for Unity", titleStyle );

        GUILayout.BeginVertical();
        {
            //Server
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Live Server Hostname: ");
                FwLive.LiveServerHostIP = EditorGUILayout.TextField("", FwLive.LiveServerHostIP, GUILayout.MinWidth(50));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            //Port
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Live Server Port: ");
                FwLive.LiveServerHostPort = EditorGUILayout.IntField("", FwLive.LiveServerHostPort, GUILayout.MinWidth(50));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                // Character Setup File
                EditorGUILayout.LabelField("Character Setup File: ");
                FwLive.ExpressionSetFile = (LiveCharacterSetupFile)EditorGUILayout.ObjectField("", FwLive.ExpressionSetFile, typeof(LiveCharacterSetupFile), false, GUILayout.MinWidth(50));
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            // Connect on Start Toggle
            FwLive.ConnectOnPlay = GUILayout.Toggle(FwLive.ConnectOnPlay, "Connect on Play");

            // Reconnect on Server Lost
            FwLive.ReconnectOnLostConnection = GUILayout.Toggle(FwLive.ReconnectOnLostConnection, new GUIContent("Automatic Reconnect", "When connection to Live Server is lost, enabling this checkbox will allow the plugin to automatically attempt reconnecting"));

            // Drop Packets Flag
            FwLive.DropPackets = GUILayout.Toggle(FwLive.DropPackets, new GUIContent("Drop Packets on Update", "Drop packets when there is more than 1 packet from Live Server queued."));

            EditorGUILayout.BeginVertical("Box"); //Begin Live Server Interface
            {
                EditorGUILayout.LabelField("Live Server", titleStyle);
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Connect"))
                    {
                        FwLive.Connect();
                    }

                    if (GUILayout.Button("Disconnect"))
                    {
                        FwLive.Disconnect();
                    }
                }
                GUILayout.EndHorizontal();
    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (FwLive.LiveServerHostIP == "localhost" ||
                    FwLive.LiveServerHostIP == "127.0.0.1")
                {
                    if (GUILayout.Button("Calibrate"))
                    {
                        FwLive.CalibrateLiveServer();
                    }
                }
    #endif
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        FwLive.EnableRecording = GUILayout.Toggle(FwLive.EnableRecording, new GUIContent("Enable Animation Recording", "Enabling Recording will set up the script to record animation coming from Live and save it to a .anim file."));

        if (FwLive.EnableRecording)
        {
            EditorGUILayout.BeginVertical("Box");
            {
                if (FwLive.Recording)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("CURRENTLY RECORDING", warningStyle);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(new GUIContent("Animation Clip:", "Animation clip that recorded animation will be saved to. If left blank, a new animation clip will be created at the root assets folder."));
                    FwLive.ClipToWriteTo = EditorGUILayout.ObjectField(FwLive.ClipToWriteTo, typeof(AnimationClip), false) as AnimationClip;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                string btnLabel = FwLive.Recording ? "Stop Recording" : "Start Recording";
                if (GUILayout.Button(btnLabel))
                {
                    if (EditorApplication.isPlaying)
                    {
                        //Create a clip to write to if user hasn't specified 
                        if (FwLive.ClipToWriteTo == null)
                        {
#if UNITY_EDITOR && (UNITY_5 || UNITY_2017 || UNITY_2018 || UNITY_5_4_OR_NEWER)
                            FwLive.ClipToWriteTo = new AnimationClip();

                            AssetDatabase.CreateAsset(FwLive.ClipToWriteTo, "Assets/" + GetNextAvailableClipName());
                            AssetDatabase.SaveAssets();
#else
                            Debug.LogWarning("[Faceware Live] No animation will be recorded due to Animation Clip not being set. Please set it to an .anim file to record animation");
#endif
                        }

                        FwLive.OnToggleRecording();
                    }
                    else
                    {
                        Debug.LogWarning("[Faceware Live] Can't start recording when not playing scene!");
                    }
                }

                FwLive.RecordOnStart = GUILayout.Toggle(FwLive.RecordOnStart, new GUIContent("Start Recording on Play", "Animation recording will start on start of the scene"));
#if UNITY_EDITOR && (UNITY_5 || UNITY_2017 || UNITY_2018 || UNITY_5_4_OR_NEWER)
                if (FwLive.RecordOnStart && FwLive.ClipToWriteTo == null)
                {
                    FwLive.ClipToWriteTo = new AnimationClip();

                    AssetDatabase.CreateAsset(FwLive.ClipToWriteTo, "Assets/" + GetNextAvailableClipName());
                    AssetDatabase.SaveAssets();
                }
#endif
                FwLive.KeyframeOnNewData = GUILayout.Toggle(FwLive.KeyframeOnNewData, new GUIContent("Keyframe only on new Data", "Enable this feature to set keyframes on controls only when their values have changed. Turning this off will yield keyframes for every control whenever Live Server sends new data, resulting is potentially very large .anim files"));

#if UNITY_EDITOR && (UNITY_5 || UNITY_2017 || UNITY_2018)
    #if UNITY_5_4_OR_NEWER
                FwLive.keyframeTangentMode = (AnimationUtility.TangentMode)EditorGUILayout.EnumPopup(new GUIContent("Animation Curves:", "Curve type used between keyframes recorded. Smoother animation can be achieved by changing this to Auto if you are getting a slower FPS from Live Server. If 'Keyframe only on new Data' is enabled, it is highly recommended to have 'Constant' curves or the end animation may not look correct"), FwLive.keyframeTangentMode);
    #endif
#endif
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            if (FwLive.Recording)
            {
                FwLive.OnToggleRecording();
            }
            FwLive.Recording = false;
        }

        EditorGUILayout.Space ();

        EditorGUILayout.BeginVertical("Box"); //Begin help buttons region
        {
            EditorGUILayout.LabelField("Need Help?", titleStyle);

            EditorGUILayout.BeginHorizontal(); //Live client user guide region
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Live Client for Unity - User Guide"))
                {
                    System.Diagnostics.Process.Start("http://support.facewaretech.com/live-client-for-unity");
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal(); //End user guide region

            EditorGUILayout.BeginHorizontal(); //Visit website region
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Visit www.facewaretech.com"))
                {
                    System.Diagnostics.Process.Start("http://www.facewaretech.com/");
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal(); //End website region

            EditorGUILayout.BeginHorizontal(); //30 day trial region
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("30-Day Free Trial", "Click here to get your 30-day free trial of Faceware Live Server.")))
                {
                    System.Diagnostics.Process.Start("http://facewaretech.com/products/software/free-trial/");
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal(); //End 30 day trial region
        }
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }
    EditorGUILayout.EndVertical ();

    if (GUI.changed)
    {
        EditorUtility.SetDirty(FwLive);
        FwLive.OnSettingsChange();
    }
  }

#if UNITY_EDITOR && (UNITY_5 || UNITY_2017 || UNITY_2018 || UNITY_5_4_OR_NEWER)
    private string GetNextAvailableClipName()
    {
        int take = 1;
        string clipName = "Faceware_Recording_";
        while (AssetDatabase.FindAssets(clipName + take.ToString()).Length > 0)
        {
            take++;
        }

        return clipName + take.ToString() + ".anim";
    }
#endif
}
