using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// SensorPoseStartupEstimator estimates the transform pose of the sensor-interface at scene start-up time.
    /// </summary>
    public class SensorPoseStartupEstimator : MonoBehaviour
    {
        [Tooltip("Depth sensor index - 0 is the 1st one, 1 - the 2nd one, etc.")]
        public int sensorIndex = 0;

        [Tooltip("Whether or not to stop the sensor pose estimation after the first successful pose update.")]
        public bool stopAfterFirstEstimate = false;

        [Tooltip("Maximum duration of the pose estimation time, in seconds.")]
        public float maxDuration = 3f;


        // reference to the KM
        private KinectManager kinectManager = null;
        private KinectInterop.SensorData sensorData = null;
        private DepthSensorInterface sensorInterface = null;
        private KinectInterop.DepthSensorPlatform sensorPlatform = KinectInterop.DepthSensorPlatform.None;

        private bool isEstimating = false;
        private bool isCompleted = false;
        private float startTime = 0f;
        private ulong startPoseTimestamp = 0;


        void Start()
        {
            kinectManager = KinectManager.Instance;
            startTime = Time.time;
        }

        void Update()
        {
            if(!isCompleted && kinectManager != null && kinectManager.IsInitialized())
            {
                if(!isEstimating)
                {
                    if(kinectManager.getPoseFrames != KinectManager.PoseUsageType.UpdateTransform)
                    {
                        // start pose estimation
                        kinectManager.EnableSensorPoseData(sensorIndex, true);
                        kinectManager.getPoseFrames = KinectManager.PoseUsageType.UpdateTransform;
                    }

                    if(sensorInterface == null)
                    {
                        // get the sensor platform
                        sensorData = kinectManager.GetSensorData(sensorIndex);
                        sensorInterface = sensorData != null ? sensorData.sensorInterface : null;
                        sensorPlatform = sensorData != null ? sensorData.sensorIntPlatform : KinectInterop.DepthSensorPlatform.None;
                    }

                    startTime = Time.time;
                    isEstimating = true;
                    
                    Debug.Log($"Pose estimation started. StartTime: {startTime}");
                }
                else if((stopAfterFirstEstimate && startPoseTimestamp != 0 && sensorData.lastSensorPoseFrameTime != startPoseTimestamp) ||
                    (maxDuration > 0f && (Time.time - startTime) >= maxDuration))
                {
                    // stop pose estimation
                    kinectManager.EnableSensorPoseData(sensorIndex, false);
                    kinectManager.getPoseFrames = KinectManager.PoseUsageType.None;
                    isCompleted = true;

                    string stopFirst = stopAfterFirstEstimate ? " after first update" : string.Empty;
                    Debug.Log($"Pose estimation stopped{stopFirst}. SensorPos: {sensorData.sensorPosePosition}, SensorRot: {sensorData.sensorPoseRotation.eulerAngles}, ts: {sensorData.lastSensorPoseFrameTime}. StopTime: {Time.time}");
                }
                else if(sensorPlatform == KinectInterop.DepthSensorPlatform.Kinect4Azure)
                {
                    // estimate the BT sensor orientation
                    float zRot = ((MonoBehaviour)sensorInterface).transform.rotation.eulerAngles.z;
                    if (zRot < 0f)
                        zRot += 360f;

                    Microsoft.Azure.Kinect.Sensor.k4abt_sensor_orientation_t btOriHint = Microsoft.Azure.Kinect.Sensor.k4abt_sensor_orientation_t.K4ABT_SENSOR_ORIENTATION_DEFAULT;
                    if (zRot > 70f && zRot < 110f)
                        btOriHint = Microsoft.Azure.Kinect.Sensor.k4abt_sensor_orientation_t.K4ABT_SENSOR_ORIENTATION_COUNTERCLOCKWISE90;
                    else if (zRot > 160f && zRot < 200f)
                        btOriHint = Microsoft.Azure.Kinect.Sensor.k4abt_sensor_orientation_t.K4ABT_SENSOR_ORIENTATION_FLIP180;
                    else if (zRot >= 250f && zRot <= 290f)
                        btOriHint = Microsoft.Azure.Kinect.Sensor.k4abt_sensor_orientation_t.K4ABT_SENSOR_ORIENTATION_CLOCKWISE90;

                    // change the BT orientation hint, if needed
                    Kinect4AzureInterface k4aInt = (Kinect4AzureInterface)sensorInterface;
                    if(btOriHint != k4aInt.bodyTrackingSensorOrientation && (int)(k4aInt.frameSourceFlags & KinectInterop.FrameSource.TypeBody) != 0)
                    {
                        Debug.Log("  stopping k4a body tracker...");
                        k4aInt.StopBodyTracking(sensorData);
                        k4aInt.bodyTrackingSensorOrientation = btOriHint;
                        k4aInt.InitBodyTracking(k4aInt.frameSourceFlags, sensorData, k4aInt.coordMapperCalib, true);
                        Debug.Log($"  k4a body tracker restarted with sensor orientation: {btOriHint}");
                    }
                }

                // set start time
                if(startPoseTimestamp == 0 && sensorData != null)
                {
                    startPoseTimestamp = sensorData.lastSensorPoseFrameTime;
                }

            }
        }

    }
}
