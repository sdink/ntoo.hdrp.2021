using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace com.rfilkov.kinect
{

    /// <summary>
    /// Body spin type.
    /// </summary>
    public enum BodySpinType : int { None = 0, FixBodySpinAndLegCross = 1, FixBodySpinOnly = 2, FixLegCrossOnly = 3 }


    /// <summary>
    /// Detects and corrects body-spins caused by wrong body-part recognitions.
    /// </summary>
    public class BodySpinFilter
    {
        // criteria to block body spinning
        private const bool IS_ALWAYS_FORWARD_FACING = true;  // whether the user is always facing the camera. otherwise uses MAX_SPIN_TIME
        private const bool SKIP_BODY_SPINS = true;  // whether to skip the body spins altogether, or to try to recover the correct pose

        private const float MAX_SPIN_TIME = 0.5f;  // in seconds, in case it's not set as always-forward-facing
        private const float MIN_ANGLE_COS = 0f;  // cos(a) used for spin detection

        private bool _fixBodySpin = true;  // whether to fix the temporary body-spin issue
        private bool _fixLegCross = true;  // whether to fix the leg-cross issue
        //private const bool FIX_JOINT_ANGLE = false;  // whether to fix the incorrect angles at knee and ankle joints

        // history data
        private BodyHistoryData[] history;


        // Initializes a new instance of the class.
        public BodySpinFilter()
        {
            Reset();
        }

        // Initializes a new instance of the class.
        public BodySpinFilter(BodySpinType bodySpinType)
        {
            switch(bodySpinType)
            {
                case BodySpinType.None:
                    _fixBodySpin = false;
                    _fixLegCross = false;
                    break;

                case BodySpinType.FixBodySpinAndLegCross:
                    _fixBodySpin = true;
                    _fixLegCross = true;
                    break;

                case BodySpinType.FixBodySpinOnly:
                    _fixBodySpin = true;
                    _fixLegCross = false;
                    break;

                case BodySpinType.FixLegCrossOnly:
                    _fixBodySpin = false;
                    _fixLegCross = true;
                    break;
            }

            Reset();
        }

        // Resets the filter to default values.
        public void Reset(ulong userId = 0)
        {
            KinectManager kinectManager = KinectManager.Instance;
            int maxBodyCount = 10;  // kinectManager.GetMaxBodyCount();
            int jointCount = kinectManager.GetJointCount();

            if(userId == 0)
            {
                // create the history data
                history = new BodyHistoryData[maxBodyCount];
                for (int i = 0; i < maxBodyCount; i++)
                {
                    history[i] = new BodyHistoryData(jointCount);
                }
            }
            else
            {
                // clean the history of the given user only
                for (int i = 0; i < maxBodyCount; i++)
                {
                    if (history[i].userId == userId)
                    {
                        history[i].userId = 0;
                        history[i].lastTimestamp = 0;
                        history[i].lastUpdateTime = 0;
                        history[i].frameCount = 0;

                        //Debug.Log("Removed history for userId " + userId + ", index: " + i);
                    }
                }
            }

        }

        // Update the filter with a new frame of data and smooth.
        public void UpdateFilter(ref KinectInterop.BodyData bodyData, long bodyTimestamp, Matrix4x4 s2wMatrix, Vector3 spaceScale)
        {
            if (bodyData.bIsTracked)
            {
                // get body index
                int bodyIndex = GetUserIndex(bodyData.liTrackingID);
                if (bodyIndex < 0)
                {
                    bodyIndex = GetFreeIndex();
                    if (bodyIndex >= 0)
                        history[bodyIndex].userId = bodyData.liTrackingID;
                    //Debug.Log("Created history for userId: " + history[bodyIndex].userId + ", index: " + bodyIndex + ", time: " + DateTime.UtcNow);
                }

                // filter
                if (bodyIndex >= 0)
                {
                    FilterBodyJoints(ref bodyData, bodyIndex, bodyTimestamp, s2wMatrix, spaceScale);
                }
            }

            // free unused history - moved to sensor-int
            //CleanUpUserHistory();
        }

        // Update the filter for all body joints
        private void FilterBodyJoints(ref KinectInterop.BodyData bodyData, int bodyIndex, long bodyTimestamp, Matrix4x4 s2wMatrix, Vector3 spaceScale)
        {
            //long nowTicks = DateTime.UtcNow.Ticks;
            long deltaTicks = bodyTimestamp - history[bodyIndex].lastTimestamp;
            float deltaTime = deltaTicks * 0.0000001f;

            // w2s matrix
            Matrix4x4 w2sMatrix = s2wMatrix.inverse;

            if (_fixBodySpin)
            {
                bool isBodyOK = CheckJointPair(ref bodyData, bodyIndex, (int)KinectInterop.JointType.ShoulderLeft, (int)KinectInterop.JointType.ShoulderRight, deltaTime, bodyTimestamp);

                if(isBodyOK)
                {
                    SaveAllJoints(ref bodyData, bodyIndex, bodyTimestamp);
                }
                else
                {
                    if (SKIP_BODY_SPINS && history[bodyIndex].frameCount > 0)
                    {
                        RestoreAllJoints(ref bodyData, bodyIndex);
                    }
                    else
                    {
                        SwapAllJoints(ref bodyData, bodyIndex, bodyTimestamp);
                        SwapAllJointsZpos(ref bodyData, s2wMatrix, spaceScale, bodyTimestamp);
                    }

                }
            }

            Vector3 hipPosL = bodyData.joint[(int)KinectInterop.JointType.HipLeft].position;
            Vector3 hipPosR = bodyData.joint[(int)KinectInterop.JointType.HipRight].position;
            Vector3 hipsDir = hipPosR - hipPosL;

            // check and fix leg-crossing issues
            if (_fixLegCross)
            {
                // check for and fix invalid l-r directions between legs
                CheckAndFixLegPair(ref bodyData, bodyIndex, (int)KinectInterop.JointType.HipLeft, (int)KinectInterop.JointType.HipRight, bodyTimestamp);
                CheckAndFixLegPair(ref bodyData, bodyIndex, (int)KinectInterop.JointType.KneeLeft, (int)KinectInterop.JointType.KneeRight, bodyTimestamp);
                CheckAndFixLegPair(ref bodyData, bodyIndex, (int)KinectInterop.JointType.AnkleLeft, (int)KinectInterop.JointType.AnkleRight, bodyTimestamp);
                CheckAndFixLegPair(ref bodyData, bodyIndex, (int)KinectInterop.JointType.FootLeft, (int)KinectInterop.JointType.FootRight, bodyTimestamp);

                CheckAndFixLegInwardDir(ref bodyData, (int)KinectInterop.JointType.KneeLeft, hipsDir, w2sMatrix, spaceScale, bodyTimestamp);
                CheckAndFixLegInwardDir(ref bodyData, (int)KinectInterop.JointType.KneeRight, -hipsDir, w2sMatrix, spaceScale, bodyTimestamp);
                //CheckAndFixLegInwardDir(ref bodyData, (int)KinectInterop.JointType.AnkleLeft, hipsDir, w2sMatrix, spaceScale, bodyTimestamp);
                //CheckAndFixLegInwardDir(ref bodyData, (int)KinectInterop.JointType.AnkleRight, -hipsDir, w2sMatrix, spaceScale, bodyTimestamp);
            }

            ////check and fix knee &ankle angles
            //if (FIX_JOINT_ANGLE)
            //{
            //    CheckAndFixLegJointAngle(ref bodyData, (int)KinectInterop.JointType.KneeLeft, -hipsDir, 35f, 180f, w2sMatrix, spaceScale, bodyTimestamp);
            //    CheckAndFixLegJointAngle(ref bodyData, (int)KinectInterop.JointType.KneeRight, -hipsDir, 35f, 180f, w2sMatrix, spaceScale, bodyTimestamp);
            //    CheckAndFixLegJointAngle(ref bodyData, (int)KinectInterop.JointType.AnkleLeft, hipsDir, 45f, 135f, w2sMatrix, spaceScale, bodyTimestamp);
            //    CheckAndFixLegJointAngle(ref bodyData, (int)KinectInterop.JointType.AnkleRight, hipsDir, 45f, 135f, w2sMatrix, spaceScale, bodyTimestamp);
            //}

            // update body root positions
            bodyData.position = bodyData.joint[0].position;
            bodyData.kinectPos = bodyData.joint[0].kinectPos;

            ////if (!isBodyOK)
            //{
            //    string sSwap = (!isBodyOK ? "1" : "0") + (isHipsSwap ? "1" : "0") +
            //        (isKneesSwap ? "1" : "0") + (isAnklesSwap ? "1" : "0") + (isFeetSwap ? "1" : "0");

            //    Vector3 shL = bodyData.joint[(int)KinectInterop.JointType.ShoulderLeft].position;
            //    Vector3 shR = bodyData.joint[(int)KinectInterop.JointType.ShoulderRight].position;
            //    Vector3 hipL = bodyData.joint[(int)KinectInterop.JointType.HipLeft].position;
            //    Vector3 hipR = bodyData.joint[(int)KinectInterop.JointType.HipRight].position;
            //    Vector3 kneeL = bodyData.joint[(int)KinectInterop.JointType.KneeLeft].position;
            //    Vector3 kneeR = bodyData.joint[(int)KinectInterop.JointType.KneeRight].position;
            //    Vector3 ankleL = bodyData.joint[(int)KinectInterop.JointType.AnkleLeft].position;
            //    Vector3 ankleR = bodyData.joint[(int)KinectInterop.JointType.AnkleRight].position;
            //    Vector3 footL = bodyData.joint[(int)KinectInterop.JointType.FootLeft].position;
            //    Vector3 footR = bodyData.joint[(int)KinectInterop.JointType.FootRight].position;

            //    Vector3 neck = bodyData.joint[(int)KinectInterop.JointType.Neck].position;
            //    Vector3 head = bodyData.joint[(int)KinectInterop.JointType.Head].position;
            //    Vector3 nose = bodyData.joint[(int)KinectInterop.JointType.Nose].position;

            //    Debug.Log($"    ts: {bodyTimestamp}, dt: {deltaTime:F6}, swap: {sSwap}, shL: {shL}, shR: {shR}, hipL: {hipL}, hipR: {hipR}, kneeL: {kneeL}, kneeR: {kneeR}, ankleL: {ankleL}, ankleR: {ankleR}, footL: {footL}, footR: {footR}, neck: {neck}, head: {head}, nose: {nose}\n");
            //}
        }

        // check the given joint pair for spinning rotation
        private bool CheckJointPair(ref KinectInterop.BodyData bodyData, int bodyIndex, int jointL, int jointR, float deltaTime, long bodyTimestamp)
        {
            bool isPairOK = true;

            Vector3 curPosL = bodyData.joint[jointL].position;
            Vector3 curPosR = bodyData.joint[jointR].position;

            Vector3 curDirLR = curPosR - curPosL;
            //curDirLR.z = -curDirLR.z;
            Vector3 prevDirLR = Vector3.right;

            // check for different directions
            float dotPrevCur = Vector3.Dot(prevDirLR.normalized, curDirLR.normalized);
            if (curDirLR != Vector3.zero && prevDirLR != Vector3.zero && dotPrevCur < MIN_ANGLE_COS && 
                (deltaTime < MAX_SPIN_TIME || IS_ALWAYS_FORWARD_FACING))
            {
                isPairOK = false;
            }

            //if (jointL == (int)KinectInterop.JointType.ShoulderLeft)
            //{
            //    string curTime = DateTime.Now.ToString("HH:mm:ss.fff");
            //    Debug.Log($"check LR for uID: {bodyData.liTrackingID} - {isPairOK}, dot: {dotPrevCur:F3}, dt: {deltaTime:F3}, time: {curTime}, ts: {bodyTimestamp}, cpL: {curPosL}, cpR: {curPosR}, cDir: ({curDirLR.x:F2}, {curDirLR.y:F2}, {curDirLR.z:F2}), pDir: ({prevDirLR.x:F2}, {prevDirLR.y:F2}, {prevDirLR.z:F2})\n");  // System.IO.File.AppendAllText(logFilename, 
            //}

            return isPairOK;
        }

        // saves all joints to history
        private void SaveAllJoints(ref KinectInterop.BodyData bodyData, int bodyIndex, long bodyTimestamp)
        {
            int jointCount = bodyData.joint.Length;

            for(int j = 0; j < jointCount; j++)
            {
                history[bodyIndex].jointHistory[j].lastPosition = bodyData.joint[j].position;
                history[bodyIndex].jointHistory[j].lastKinectPos = bodyData.joint[j].kinectPos;
                history[bodyIndex].jointHistory[j].lastTrackingState = bodyData.joint[j].trackingState;
            }

            history[bodyIndex].lastTimestamp = (long)bodyData.bodyTimestamp;
            history[bodyIndex].lastUpdateTime = DateTime.UtcNow.Ticks;
            history[bodyIndex].frameCount++;

            //string curTime = DateTime.Now.ToString("HH:mm:ss.fff");
            //Debug.Log($"  saved joints - uID: {bodyData.liTrackingID} time: {curTime}, ts: {bodyTimestamp}\n");  // System.IO.File.AppendAllText(logFilename, 
        }

        // restores all joints from history
        private void RestoreAllJoints(ref KinectInterop.BodyData bodyData, int bodyIndex)
        {
            int jointCount = bodyData.joint.Length;

            for (int j = 0; j < jointCount; j++)
            {
                bodyData.joint[j].position = history[bodyIndex].jointHistory[j].lastPosition;
                bodyData.joint[j].kinectPos = history[bodyIndex].jointHistory[j].lastKinectPos;
                bodyData.joint[j].trackingState = history[bodyIndex].jointHistory[j].lastTrackingState;
            }

            // restore body timestamp
            bodyData.bodyTimestamp = (ulong)history[bodyIndex].lastTimestamp;

            // prevent history clean ups
            history[bodyIndex].lastUpdateTime = DateTime.UtcNow.Ticks;

            //string curTime = DateTime.Now.ToString("HH:mm:ss.fff");
            //Debug.Log($"  restored joints - uID: {bodyData.liTrackingID}, ts: {history[bodyIndex].lastTimestamp}, time: {curTime}\n");  // System.IO.File.AppendAllText(logFilename, 
        }

        // swaps all left & right joints
        private void SwapAllJoints(ref KinectInterop.BodyData bodyData, int bodyIndex, long bodyTimestamp)
        {
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.ClavicleLeft, (int)KinectInterop.JointType.ClavicleRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.ShoulderLeft, (int)KinectInterop.JointType.ShoulderRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.ElbowLeft, (int)KinectInterop.JointType.ElbowRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.WristLeft, (int)KinectInterop.JointType.WristRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.HandLeft, (int)KinectInterop.JointType.HandRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.HandtipLeft, (int)KinectInterop.JointType.HandtipRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.ThumbLeft, (int)KinectInterop.JointType.ThumbRight, bodyIndex);

            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.HipLeft, (int)KinectInterop.JointType.HipRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.KneeLeft, (int)KinectInterop.JointType.KneeRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.AnkleLeft, (int)KinectInterop.JointType.AnkleRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.FootLeft, (int)KinectInterop.JointType.FootRight, bodyIndex);

            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.EyeLeft, (int)KinectInterop.JointType.EyeRight, bodyIndex);
            SwapJointsData(ref bodyData, (int)KinectInterop.JointType.EarLeft, (int)KinectInterop.JointType.EarRight, bodyIndex);

            //string curTime = DateTime.Now.ToString("HH:mm:ss.fff");
            //Debug.Log($"  swapped joints - uID: {bodyData.liTrackingID}, ts: {bodyData.bodyTimestamp}, time: {curTime}\n");  // System.IO.File.AppendAllText(logFilename, 
        }

        // restores all joints from history
        private void SwapAllJointsZpos(ref KinectInterop.BodyData bodyData, Matrix4x4 s2wMatrix, Vector3 spaceScale, long bodyTimestamp)
        {
            float pelPosZ = bodyData.joint[(int)KinectInterop.JointType.Pelvis].kinectPos.z;
            int jointCount = bodyData.joint.Length;

            for (int j = 1; j < jointCount; j++)
            {
                int joint = j;

                Vector3 kinectPos = bodyData.joint[joint].kinectPos;
                float jointDiffZ = kinectPos.z - pelPosZ;
                kinectPos.z -= 2 * jointDiffZ;

                bodyData.joint[joint].kinectPos = kinectPos;
                bodyData.joint[joint].position = s2wMatrix.MultiplyPoint3x4(new Vector3(kinectPos.x * spaceScale.x, kinectPos.y * spaceScale.y, kinectPos.z));
            }

            //string curTime = DateTime.Now.ToString("HH:mm:ss.fff");
            //Debug.Log($"  swapZpos joints - uID: {bodyData.liTrackingID}, ts: {bodyTimestamp}, time: {curTime}\n");  // System.IO.File.AppendAllText(logFilename, 
        }

        // checks the given leg pair for incorrect direction, and fixes it if needed
        private void CheckAndFixLegPair(ref KinectInterop.BodyData bodyData, int bodyIndex, int jointL, int jointR, long bodyTimestamp)
        {
            bool isPairOK = true;

            Vector3 legPosL = bodyData.joint[jointL].position;
            Vector3 legPosR = bodyData.joint[jointR].position;
            Vector3 legDirLR = legPosR - legPosL;
            legDirLR.z = -legDirLR.z;

            Vector3 shPosL = bodyData.joint[(int)KinectInterop.JointType.ShoulderLeft].position;
            Vector3 shPosR = bodyData.joint[(int)KinectInterop.JointType.ShoulderRight].position;
            Vector3 shDirLR = shPosR - shPosL;
            shDirLR.z = -shDirLR.z;

            // check for different directions
            float dotShLeg = Vector3.Dot(shDirLR.normalized, legDirLR.normalized);
            if (legDirLR != Vector3.zero && shDirLR != Vector3.zero && dotShLeg < 0f)
            {
                isPairOK = false;
            }

            //if (jointL == (int)KinectInterop.JointType.KneeLeft)
            //{
            //    string curTime = DateTime.Now.ToString("HH:mm:ss.fff");
            //    Debug.Log($"time: {curTime}, dot: {dotPrevCur:F3}, lpL: {legPosL}, lpR: {legPosR}, lDir: {legDirLR}, pDir: {hipDirLR}\n");  // System.IO.File.AppendAllText(logFilename, 
            //}

            if (!isPairOK)
            {
                // fix the issue
                SwapJointsData(ref bodyData, jointL, jointR, bodyIndex);
                //Debug.Log($"  swapping {(KinectInterop.JointType)jointL}-{(KinectInterop.JointType)jointR} for uID: {bodyData.liTrackingID}, ts: {bodyTimestamp}, shDir: {shDirLR}, legDir: {legDirLR}, dot: {dotShLeg:F3}\n");  // System.IO.File.AppendAllText(logFilename, 
            }
        }

        // swaps the positional data of two joints
        private void SwapJointsData(ref KinectInterop.BodyData bodyData, int jointL, int jointR, int bodyIndex)
        {
            KinectInterop.TrackingState trackingStateL = bodyData.joint[jointL].trackingState;
            Vector3 kinectPosL = bodyData.joint[jointL].kinectPos;
            Vector3 positionL = bodyData.joint[jointL].position;

            KinectInterop.TrackingState trackingStateR = bodyData.joint[jointR].trackingState;
            Vector3 kinectPosR = bodyData.joint[jointR].kinectPos;
            Vector3 positionR = bodyData.joint[jointR].position;

            bodyData.joint[jointL].trackingState = trackingStateR;
            bodyData.joint[jointL].kinectPos = kinectPosR;
            bodyData.joint[jointL].position = positionR;

            bodyData.joint[jointR].trackingState = trackingStateL;
            bodyData.joint[jointR].kinectPos = kinectPosL;
            bodyData.joint[jointR].position = positionL;
        }

        //// checks the given leg joint for incorrect angle, and fixes it if needed
        //private void CheckAndFixLegJointAngle(ref KinectInterop.BodyData bodyData, int midJoint, Vector3 hipsDir, float minAngle, float maxAngle, 
        //    Matrix4x4 w2sMatrix, Vector3 spaceScale, long bodyTimestamp)
        //{
        //    int parJoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)midJoint);
        //    int nextJoint = (int)KinectInterop.GetNextJoint((KinectInterop.JointType)midJoint);

        //    if(bodyData.joint[midJoint].trackingState == KinectInterop.TrackingState.NotTracked || 
        //        bodyData.joint[parJoint].trackingState == KinectInterop.TrackingState.NotTracked ||
        //        bodyData.joint[nextJoint].trackingState == KinectInterop.TrackingState.NotTracked)
        //    {
        //        return;
        //    }

        //    Vector3 midJointPos = bodyData.joint[midJoint].position;
        //    Vector3 parJointPos = bodyData.joint[parJoint].position;
        //    Vector3 nextJointPos = bodyData.joint[nextJoint].position;

        //    Vector3 parJointDir = parJointPos - midJointPos;
        //    Vector3 nextJointDir = nextJointPos - midJointPos;

        //    // check the angle
        //    float dirAngle = Vector3.SignedAngle(parJointDir.normalized, nextJointDir.normalized, hipsDir.normalized);
        //    //Debug.Log($"  {(KinectInterop.JointType)midJoint} for uID {bodyData.liTrackingID} - dirs-angle: {dirAngle:F1}, parDir: {parJointDir}, nextDir: {nextJointDir}, hipsDir: {hipsDir}, min: {minAngle}, max: {maxAngle}");

        //    if (parJointDir != Vector3.zero && nextJointDir != Vector3.zero && hipsDir != Vector3.zero &&
        //        (dirAngle < minAngle || dirAngle > maxAngle))
        //    {
        //        Vector3 crossDir = Vector3.Cross(parJointDir.normalized, nextJointDir.normalized);
        //        float turnAngle = Mathf.Abs(Mathf.DeltaAngle(dirAngle, minAngle)) < Mathf.Abs(Mathf.DeltaAngle(dirAngle, maxAngle)) ? minAngle : maxAngle;
        //        Quaternion turnRotation = Quaternion.AngleAxis(turnAngle, crossDir.normalized);
                
        //        Vector3 newJointDir = turnRotation * parJointDir;
        //        newJointDir *= nextJointDir.magnitude / parJointDir.magnitude;  // scale

        //        Vector3 newJointPos = midJointPos + newJointDir;
        //        bodyData.joint[nextJoint].position = newJointPos;

        //        Vector3 newKinectPos = w2sMatrix.MultiplyPoint3x4(newJointPos);
        //        bodyData.joint[nextJoint].kinectPos = new Vector3(newKinectPos.x * spaceScale.x, newKinectPos.y * spaceScale.y, newKinectPos.z);

        //        Debug.Log($"  fix angle @ {(KinectInterop.JointType)midJoint} for uID {bodyData.liTrackingID} - old: {dirAngle:F1} new: {turnAngle:F1}, ts: {bodyTimestamp}, newDir: {newJointDir}, newPos: {newJointPos}\n");  // System.IO.File.AppendAllText(logFilename, 
        //    }
        //}

        // checks the given leg joint for incorrect (inward) direction, and fixes it if needed
        private void CheckAndFixLegInwardDir(ref KinectInterop.BodyData bodyData, int midJoint, Vector3 hipsDir, Matrix4x4 w2sMatrix, Vector3 spaceScale, long bodyTimestamp)
        {
            int parJoint = (int)KinectInterop.GetParentJoint((KinectInterop.JointType)midJoint);

            if (bodyData.joint[midJoint].trackingState == KinectInterop.TrackingState.NotTracked ||
                bodyData.joint[parJoint].trackingState == KinectInterop.TrackingState.NotTracked)
            {
                return;
            }

            Vector3 midJointPos = bodyData.joint[midJoint].position;
            Vector3 parJointPos = bodyData.joint[parJoint].position;
            Vector3 parJointDir = midJointPos - parJointPos;

            Vector3 hipsBackDir = Vector3.Cross(hipsDir.normalized, parJointDir.normalized);
            float dotJointDir = Vector3.Dot(hipsDir.normalized, parJointDir.normalized);

            if(dotJointDir > 0f)
            {
                // fix the joint position
                Vector3 newJointDir = Vector3.Cross(hipsBackDir.normalized, hipsDir.normalized);
                newJointDir *= parJointDir.magnitude;

                Vector3 newJointPos = parJointPos + newJointDir;
                bodyData.joint[midJoint].position = newJointPos;
                Vector3 newKinectPos = w2sMatrix.MultiplyPoint3x4(newJointPos);
                bodyData.joint[midJoint].kinectPos = new Vector3(newKinectPos.x * spaceScale.x, newKinectPos.y * spaceScale.y, newKinectPos.z);

                //Debug.Log($"  fix inward @ {(KinectInterop.JointType)midJoint} for uID {bodyData.liTrackingID} - dot: {dotJointDir:F3} ts: {bodyTimestamp}, oldDir: {parJointDir}, newDir: {newJointDir}, newPos: {newJointPos}\n");  // System.IO.File.AppendAllText(logFilename, 
            }
        }

        // returns the history index for the given user, or -1 if not found
        private int GetUserIndex(ulong userId)
        {
            for (int i = 0; i < history.Length; i++)
            {
                if (history[i].userId == userId)
                    return i;
            }

            return -1;
        }

        // returns the 1st free history index, or -1 if not found
        private int GetFreeIndex()
        {
            for (int i = 0; i < history.Length; i++)
            {
                if (history[i].userId == 0)
                    return i;
            }

            return -1;
        }

        // frees history indices that were unused for long time
        public void CleanUpUserHistory()
        {
            DateTime dtNow = DateTime.UtcNow;
            long timeNow = dtNow.Ticks;

            for (int i = 0; i < history.Length; i++)
            {
                if (history[i].userId != 0 && (timeNow - history[i].lastUpdateTime) >= 10000000)
                {
                    //Debug.Log("Removing history for userId " + history[i].userId + ", index: " + i + ", time: " + dtNow + ", not used since: " + (timeNow - history[i].lastUpdateTime) + " ticks");

                    history[i].userId = 0;
                    history[i].lastTimestamp = 0;
                    history[i].lastUpdateTime = 0;
                    history[i].frameCount = 0;
                }
            }
        }


        // body history data used by the filter
        private struct BodyHistoryData
        {
            public ulong userId;
            public long lastTimestamp;
            public long lastUpdateTime;
            public JointHistoryData[] jointHistory;
            public uint frameCount;


            public BodyHistoryData(int jointCount)
            {
                userId = 0;
                lastTimestamp = 0;
                lastUpdateTime = 0;
                jointHistory = new JointHistoryData[jointCount];
                frameCount = 0;
            }
        }

        // joint history data used by the filter
        private struct JointHistoryData
        {
            // last joint position  
            public Vector3 lastPosition;

            // last sensor position  
            public Vector3 lastKinectPos;

            // last tracking state
            public KinectInterop.TrackingState lastTrackingState;
        }

    }
}

