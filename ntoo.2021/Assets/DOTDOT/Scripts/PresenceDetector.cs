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

    private ulong userId;
    private Vector3 neutralEyeTarget;

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
            userPresent = false;
            OnNoUsersPresent.Invoke();
            if (headTrackingTarget != null)
            {
                onHeadTrackingActive.Invoke(false);
                headTrackingTarget.localPosition = neutralEyeTarget;
            }
        }
        else
        {
            userId = presentUsers.First();
        }
    }

    private void Update()
    {
        if (userPresent && headTrackingTarget != null)
        {
            if (kinectManager.GetJointTrackingState(userId, KinectInterop.JointType.Head) == KinectInterop.TrackingState.Tracked)
            {
                var pos = kinectManager.GetJointPosition(userId, KinectInterop.JointType.Head);
                headTrackingTarget.localPosition = new Vector3(-pos.x, pos.y, pos.z); // invert x-axis to remove mirroring
                onHeadTrackingActive.Invoke(true);
            }
            else
            {
                onHeadTrackingActive.Invoke(false);
                headTrackingTarget.localPosition = neutralEyeTarget;
            }
        }
    }
}
