using com.rfilkov.kinect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.rfilkov.components
{
    /// <summary>
    /// Displays userId, positional and rotational information for the specified user on screen. 
    /// </summary>
    public class DisplayUserInfo : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("UI Text to display debug information.")]
        public UnityEngine.UI.Text debugText;


        void Update()
        {
            KinectManager kinectManager = KinectManager.Instance;
            if (debugText != null && kinectManager != null && kinectManager.IsInitialized())
            {
                if (kinectManager.IsUserDetected(playerIndex))
                {
                    ulong userId = kinectManager.GetUserIdByIndex(playerIndex);
                    Vector3 userPos = kinectManager.GetUserPosition(userId);
                    Vector3 userSensorPos = kinectManager.GetUserKinectPosition(userId, true);
                    Vector3 userRot = kinectManager.GetUserOrientation(userId, true).eulerAngles;

                    Vector3 headRot = kinectManager.GetJointOrientation(userId, KinectInterop.JointType.Head, true).eulerAngles;
                    Vector3 neckRot = kinectManager.GetJointOrientation(userId, KinectInterop.JointType.Neck, true).eulerAngles;

                    string sText = $"User: {userId}, Pos: {userPos.ToString("F2")}, KPos: {userSensorPos.ToString("F2")}, Rotation: {userRot.ToString("F0")}" +
                        $"\nHeadRot: {headRot.ToString("F0")}, NeckRot: {neckRot.ToString("F0")}";
                    debugText.text = sText;
                }
                else
                {
                    debugText.text = string.Empty;
                }
            }
        }

    }
}
