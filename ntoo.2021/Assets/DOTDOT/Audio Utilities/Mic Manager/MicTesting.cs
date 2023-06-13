using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicTesting : MonoBehaviour
{
    private int micBuffer = 2;

    private AudioClip micOutput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        micOutput = Microphone.Start(null, true, micBuffer, 44100);
    }

    private void OnDisable()
    {
        Microphone.End(null);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log("[Mic Tester] Mic position: " + Microphone.GetPosition(null).ToString());
    }
}
