using UnityEngine;

public class MicUiController : MonoBehaviour
{
    [SerializeField] private GameObject StartRecordingButton;
    [SerializeField] private GameObject StopRecordingButton;
    [SerializeField] private GameObject MicrophoneWarning;
    [SerializeField] private GameObject RecordingInfo;

    public void WarnNoMicrophone()
    {
        // Throw a warning message at the console.
        // !debug - Not necessary during testing while I know I have no mic plugged in.
        Debug.LogWarning("Microphone not connected!");

        StartRecordingButton.SetActive(false);
        MicrophoneWarning.SetActive(true);
    }

    public void StartedRecording()
    {
        StartRecordingButton.SetActive(false);
        StopRecordingButton.SetActive(true);
        RecordingInfo.SetActive(true);
    }

    public void StoppedRecording()
    {
        StopRecordingButton.SetActive(false);
        RecordingInfo.SetActive(false);
        StartRecordingButton.SetActive(true);
    }
}
