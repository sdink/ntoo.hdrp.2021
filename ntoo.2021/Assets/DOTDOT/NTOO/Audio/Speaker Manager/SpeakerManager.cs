using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;

public class SpeakerManager : MonoBehaviour
{
  // Settings
  [SerializeField] private float speakerVolume = 1;

  [Serializable] private class NtooEvent : UnityEvent { }
  [Header("Ntoo Manager Events")]
  [SerializeField] private NtooEvent OnStoppedSpeaking;

  public bool userPresent { get; set; }

  // Internal
  private AudioSource audioSource;

  private Coroutine audioPlaybackTimeout;

  // Start is called before the first frame update
  void Start()
  {
    audioSource = GetComponent<AudioSource>();
    audioSource.loop = false;
    audioSource.mute = false;
    audioSource.volume = speakerVolume;
  }

  /// <summary>
  /// Play the given audio clip to the speaker.
  /// </summary>
  /// <param name="audioClip"></param>
  public void StartSpeaking(AudioClip audioClip)
  {
    if (audioPlaybackTimeout != null)
    {
      Debug.Log("[Speaker Manager] Already speaking, skipping this request");
    }
    else
    {
      Debug.Log($"[Speaker Manager] Started speaking with audio clip of length {audioClip.length} and frequency {audioClip.frequency}.");
      audioSource.clip = audioClip;
      audioSource.Play();
      audioPlaybackTimeout = StartCoroutine(PlayAudioCallback(audioClip.length));
    }
  }
  /// <summary>
  /// Disable the Speaker Visualiser UI after the given amount of time.
  /// </summary>
  /// <param name="clipLength"></param>
  /// <returns></returns>
  private IEnumerator PlayAudioCallback(float clipLength)
  {
    // Note: This is [DEBUG] because the amount of time it waits
    // is oftentimes significantly longer than the clipLength and I
    // can't figure out why. It doesn't seem to be a linear relationship.
    Debug.Log("[Speaker Manager] Waiting for " + clipLength + " seconds...");
    yield return new WaitForSecondsRealtime(clipLength);
    audioPlaybackTimeout = null;
    Debug.Log("[Speaker Manager] Clip length timeout reached");
    OnStoppedSpeaking.Invoke();
  }

  private void Update()
  {
    if (audioPlaybackTimeout != null && !audioSource.isPlaying)
    {
      Debug.Log("[Speaker Manager] Detected audio stop before timeout - cancelling early");
      StopCoroutine(audioPlaybackTimeout);
      audioPlaybackTimeout = null;
      OnStoppedSpeaking.Invoke();
    }
  }

  public void StopSpeaking()
  {
    Debug.Log($"[Speaker Manager] Stopped speaking.");
    audioSource.Stop();
  }
}
