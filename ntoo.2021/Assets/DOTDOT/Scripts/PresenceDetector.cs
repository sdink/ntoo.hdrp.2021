using com.rfilkov.kinect;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PresenceDetector : MonoBehaviour
{
    private KinectManager kinectManager;

    public UnityEvent OnUserPresent;

    public UnityEvent OnNoUsersPresent;

    private bool userPresent = false;

    private HashSet<ulong> presentUsers = new HashSet<ulong>();

    [Header("Head Tracking")]
    [SerializeField]
    [Tooltip("Optional transform to update for head tracking of present user")]
    private Transform headTrackingTarget;

    [SerializeField]
    [Tooltip("Triggered when optional head tracking target is valid")]
    private UnityEvent<bool> onHeadTrackingActive;

    [SerializeField]
    [Tooltip("How long eyes should linger when losing tracking of a user before shifting to next user")]
    private float lingerPeriod = 1.5f;

    [SerializeField]
    [Tooltip("Time in seconds it should take for eye to move from one position to the next. Higher values result in smoother eye movements that trail behind users")]
    private float eyeMoveLag = 0.5f;

    private ulong userId;
    private Vector3 neutralEyeTarget;
    private Vector3 eyeTarget;

    float linger = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (KinectManager.Instance == null)
        {
            Debug.LogWarning("[Presence Detector] Unable to find Kinect Manager instance!");
            enabled = false;
            return;
        }

        kinectManager = KinectManager.Instance;

        kinectManager.userManager.OnUserAdded.AddListener(HandleUserDetected);
        kinectManager.userManager.OnUserRemoved.AddListener(HandleUserLost);

        if (headTrackingTarget != null)
        {
            neutralEyeTarget = headTrackingTarget.localPosition;
            eyeTarget = headTrackingTarget.localPosition;
        }
    }

    private void HandleUserDetected(ulong id, int userIndex)
    {
        presentUsers.Add(id);
        if (!userPresent)
        {
            userPresent = true;
            userId = id;
            OnUserPresent.Invoke();
        }
    }

    private void HandleUserLost(ulong id, int userIndex)
    {
        presentUsers.Remove(id);
        if (presentUsers.Count == 0)
        {
            linger = eyeMoveLag;
            userPresent = false;
            OnNoUsersPresent.Invoke();
            if (headTrackingTarget != null)
            {
                onHeadTrackingActive.Invoke(false);
                eyeTarget = neutralEyeTarget;
            }
        }
        else if (userId == id)
        {
            userId = presentUsers.First();
            linger = eyeMoveLag;
        }
    }

    private void Update()
    {
        if (headTrackingTarget != null)
        {

            if (linger > 0)
            {
                linger -= Time.deltaTime;
            }
            else if (!userPresent)
            {
                eyeTarget = neutralEyeTarget;
            }
            else if (kinectManager.GetJointTrackingState(userId, KinectInterop.JointType.Head) == KinectInterop.TrackingState.Tracked)
            {
                var pos = kinectManager.GetJointPosition(userId, KinectInterop.JointType.Head);
                eyeTarget = new Vector3(-pos.x, pos.y, pos.z); // invert x-axis to remove mirroring
                onHeadTrackingActive.Invoke(true);
            }
            else
            {
                onHeadTrackingActive.Invoke(false);
                eyeTarget = neutralEyeTarget;
            }

            headTrackingTarget.localPosition = Vector3.Lerp(headTrackingTarget.localPosition, eyeTarget, Time.deltaTime / eyeMoveLag);
        }
    }
}
