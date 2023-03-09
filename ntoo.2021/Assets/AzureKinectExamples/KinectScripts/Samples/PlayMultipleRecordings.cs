using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;

namespace com.rfilkov.components
{
    /// <summary>
    /// This component tries to play back multiple recording files, if applicable.
    /// </summary>
    public class PlayMultipleRecordings : MonoBehaviour
    {
        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("List of full paths to recording files that need to be played back.")]
        public string[] recordingFiles = new string[0];


        // references to KM & sensor data
        private KinectManager kinectManager = null;
        private DepthSensorBase sensorInterface = null;

        private int recFileIndex = -1;
        private ulong lastDepthFrameTime = 0;
        private float lastUnityTime = 0f;
        private bool isGoToNextRec = false;


        void Update()
        {
            // find the KinectManager-component in the scene
            if(kinectManager == null)
            {
                kinectManager = FindObjectOfType<KinectManager>();

                if(kinectManager == null)
                {
                    Debug.LogError("Can't find the KinectManager-component! Please check your scene setup.");
                }
            }

            // find the sensor-interface component in the scene
            if(sensorInterface == null)
            {
                DepthSensorBase[] sensorInterfaces = FindObjectsOfType<DepthSensorBase>();

                for(int i = 0; i < sensorInterfaces.Length; i++)
                {
                    if(sensorInterfaces[i].deviceStreamingMode != KinectInterop.DeviceStreamingMode.Disabled)
                    {
                        sensorInterface = sensorInterfaces[i];
                    }
                }

                if (sensorInterface == null)
                {
                    Debug.LogError("Can't find any active sensor interface component! Please check your scene setup.");
                }
            }

            if (kinectManager && sensorInterface && recordingFiles.Length > 0)
            {
                // update the recording file if needed
                if(isGoToNextRec || recFileIndex < 0)
                {
                    // stop depth sensor
                    if (kinectManager.IsInitialized())
                    {
                        Debug.Log("Stopping depth sensors...");
                        kinectManager.StopDepthSensors();
                    }

                    // change the recording file
                    recFileIndex = (recFileIndex + 1) % recordingFiles.Length;
                    string recordingFile = recordingFiles[recFileIndex];

                    sensorInterface.deviceStreamingMode = KinectInterop.DeviceStreamingMode.PlayRecording;
                    sensorInterface.recordingFile = recordingFile;
                    Debug.Log("Setting new playback file: " + recordingFile);

                    if(kinectManager && kinectManager.statusInfoText)
                    {
                        kinectManager.statusInfoText.text = string.Empty;
                    }

                    // restart depth sensor
                    Debug.Log("Restarting depth sensors...");
                    kinectManager.StartDepthSensors();

                    lastUnityTime = Time.time;
                    isGoToNextRec = false;
                }

                if(lastDepthFrameTime != kinectManager.GetDepthFrameTime(sensorIndex))
                {
                    lastDepthFrameTime = kinectManager.GetDepthFrameTime(sensorIndex);
                    lastUnityTime = Time.time;
                }
                else
                {
                    if((Time.time - lastUnityTime) >= 3f)  // no depth data for 3 seconds
                    {
                        Debug.Log("Timed out. Switch to the next recording.");
                        isGoToNextRec = true;
                    }
                }

            }
        }

    }
}
