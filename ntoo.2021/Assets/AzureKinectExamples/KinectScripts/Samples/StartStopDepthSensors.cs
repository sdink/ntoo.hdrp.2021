using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;

namespace com.rfilkov.components
{
    /// <summary>
    /// This component tries to stop and restart the camera(s) after some time.
    /// </summary>
    public class StartStopDepthSensors : MonoBehaviour
    {
        [Tooltip("Stop depth sensors after this amount of seconds.")]
        public float stopAfterSeconds = 10f;

        [Tooltip("Restart depth sensors after this amount of seconds.")]
        public float restartAfterSeconds = 5f;


        // references to KM & sensor data
        private KinectManager kinectManager = null;


        void Start()
        {
            // look for the KinectManager-component in the scene
            kinectManager = FindObjectOfType<KinectManager>();

            if (kinectManager != null)
            {
                StartCoroutine(StopAndRestartSensors());
            }
            else
            {
                Debug.LogError("Can't find the KinectManager-component! Please check your scene setup.");
            }

        }


        // stops and then restarts the depth sensors
        private IEnumerator StopAndRestartSensors()
        {
            while(true)
            {
                // wait for seconds before stop
                Debug.Log("Waiting for " + stopAfterSeconds + " seconds...");
                yield return new WaitForSeconds(stopAfterSeconds);

                Debug.Log("Stopping depth sensors...");
                kinectManager.StopDepthSensors();

                // wait for seconds before restart
                Debug.Log("Waiting for " + restartAfterSeconds + " seconds...");
                yield return new WaitForSeconds(restartAfterSeconds);

                // restart depth sensor
                Debug.Log("Restarting depth sensors...");
                kinectManager.StartDepthSensors();
            }
        }

    }
}
