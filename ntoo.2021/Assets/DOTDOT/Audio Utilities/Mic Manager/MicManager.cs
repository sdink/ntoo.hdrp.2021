using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]

public class MicManager : MonoBehaviour
{
    // UI Events
    [Serializable] private class UIEvent : UnityEvent { }
    [Header("UI Events")]
    [SerializeField] private UIEvent UINoMicrophoneDetected;
    [SerializeField] private UIEvent UIStartedRecording;
    [SerializeField] private UIEvent UIStoppedRecording;

    // Client Events
    [Serializable] private class ClientEvent : UnityEvent<AudioClip> { }
    [Header("Client Events")]
    [SerializeField] private ClientEvent ClientStreamingAudio;

    // The maximum and minimum available recording frequencies.
    private int minFreq;
    private int maxFreq;

    // The selected mic index.
    private int selectedMic;
  
    // A handle to the attached AudioSource.
    private AudioSource audioSource;

    // Use this for initialization.
    void Start()   
    {
        // Check if there is at least one microphone connected.
        if (Microphone.devices.Length <= 0)  
        {
            UINoMicrophoneDetected.Invoke();
        }
        else // At least one microphone is available.
        {
            // Get the default microphone recording capabilities  
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);  
  
            // According to the documentation, if minFreq and maxFreq are zero,
            // the microphone supports any frequency...  
            if (minFreq == 0 && maxFreq == 0)  
            {  
                // ...meaning 44100 Hz can be used as the recording sampling rate.
                maxFreq = 44100;
            }
  
            // Get the attached AudioSource component  
            audioSource = this.GetComponent<AudioSource>();
        }
    }

    public void StartRecording()
    {
        UIStartedRecording.Invoke();
        audioSource.clip = Microphone.Start(Microphone.devices[selectedMic], true, 20, maxFreq);
    }

    public void StopRecording()
    {
        // Update the UI and Unity Mic to stop recording.
        UIStoppedRecording.Invoke();
        Microphone.End(null); // Stop the audio recording

        // For debug use:
        //audioSource.Play(); // Playback the recorded audio

        // Send the Audio Clip to the Client Manager for streaming.
        ClientStreamingAudio.Invoke(audioSource.clip);
    }

    public void PlayAudio(AudioClip audioClip)
    {
        // This overwrites the input audioClip, as it
        // isn't necessary to maintain both concurrently.
        Debug.Log($"Playing audioClip");
        audioSource.clip = audioClip;
        audioSource.Play();
    }
    public void SetMicrophone(Int32 micIndex)
    {
        if (micIndex > 0 && micIndex < Microphone.devices.Length)
        {
            selectedMic = micIndex;
            PlayerPrefs.SetInt("selectedMic", micIndex);
        }
    }
}