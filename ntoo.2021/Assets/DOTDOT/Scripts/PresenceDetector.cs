using com.rfilkov.kinect;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PresenceDetector : MonoBehaviour
{
    private KinectManager kinectManager;

    public UnityEvent OnUserPresent;

    public UnityEvent OnNoUsersPresent;

    private bool userPresent = false;

    private HashSet<ulong> presentUsers = new HashSet<ulong>();

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
    }

    private void HandleUserDetected(ulong id, int userIndex)
    {
        presentUsers.Add(id);
        if (!userPresent)
        {
            userPresent = true;
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
        }
    }
}
