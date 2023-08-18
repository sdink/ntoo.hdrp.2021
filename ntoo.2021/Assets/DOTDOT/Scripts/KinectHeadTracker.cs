using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;

public class KinectHeadTracker : MonoBehaviour
{


    [SerializeField]
    private Transform headTarget;

    private ulong userId = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (headTarget == null) headTarget = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (KinectManager.Instance != null)
        {
            if (KinectManager.Instance.GetJointTrackingState(userId, KinectInterop.JointType.Head) == KinectInterop.TrackingState.Tracked)
            {
                var position = KinectManager.Instance.GetJointPosition(userId, KinectInterop.JointType.Head);
                transform.localPosition = position;
                transform.gameObject.SetActive(true);
            }
            else
            {
                transform.gameObject.SetActive(false);
            }
        }
    }
}
