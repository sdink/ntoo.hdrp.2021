using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    public class JointValuesSaver : MonoBehaviour
    {
        public string fileName = "saved_joints.csv";

        public KinectInterop.JointType[] joints =
        {
        KinectInterop.JointType.Pelvis,
        KinectInterop.JointType.ShoulderLeft,
        KinectInterop.JointType.ShoulderRight,
        KinectInterop.JointType.HipLeft,
        KinectInterop.JointType.HipRight,
        KinectInterop.JointType.KneeLeft,
        KinectInterop.JointType.KneeRight,
        KinectInterop.JointType.AnkleLeft,
        KinectInterop.JointType.AnkleRight,
        KinectInterop.JointType.FootLeft,
        KinectInterop.JointType.FootRight
    };

        public UnityEngine.UI.Text infoText;


        private const char delim = ',';
        private KinectManager kinectManager;

        // Start is called before the first frame update
        void Start()
        {
            kinectManager = KinectManager.Instance;

            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (kinectManager && kinectManager.IsUserDetected(0))
            {
                ulong userId = kinectManager.GetUserIdByIndex(0);
                System.Text.StringBuilder sbBuf = new System.Text.StringBuilder();

                // header
                if (!System.IO.File.Exists(fileName))
                {
                    sbBuf.Append("Time");
                    foreach (var joint in joints)
                    {
                        sbBuf.Append(delim).AppendFormat("{0}-P", joint);
                        sbBuf.Append(delim).AppendFormat("{0}-R", joint);
                    }

                    sbBuf.AppendLine();
                    System.IO.File.AppendAllText(fileName, sbBuf.ToString());
                    sbBuf.Clear();
                }

                float curTime = Time.time;
                sbBuf.AppendFormat("{0:F3}", curTime);
                if (infoText)
                    infoText.text = "Time: " + curTime;

                foreach (var joint in joints)
                {
                    bool jTracked = kinectManager.IsJointTracked(userId, joint);
                    Vector3 jPos = kinectManager.GetJointPosition(userId, joint);
                    Vector3 jRot = kinectManager.GetJointOrientation(userId, joint, true).eulerAngles;

                    string sTracked = jTracked ? "1" : "0";
                    sbBuf.Append(";").AppendFormat("{0}/({1:F2}|{2:F2}|{3:F2})", sTracked, jPos.x, jPos.y, jPos.z);

                    //sbBuf.Append(";").AppendFormat("({0:F0}|{1:F0}|{2:F0})", jRot.x, jRot.y, jRot.z);
                    sbBuf.Append(";").AppendFormat("{0:F0}", jRot.y);
                }

                sbBuf.AppendLine();
                sbBuf = sbBuf.Replace(',', '.').Replace(';', delim);

                System.IO.File.AppendAllText(fileName, sbBuf.ToString());
                sbBuf.Clear();
            }
        }

    }
}
