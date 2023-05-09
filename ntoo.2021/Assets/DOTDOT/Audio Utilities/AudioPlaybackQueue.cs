using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlaybackQueue : MonoBehaviour
{
    AudioSource playbackTarget;

    Queue<AudioClip> audioClips = new Queue<AudioClip>();


    // Start is called before the first frame update
    void Start()
    {
        playbackTarget = GetComponent<AudioSource>();
    }

    public void PlayOrQueueAudio(AudioClip clip)
    {
        if (playbackTarget.isPlaying)
        {
            audioClips.Enqueue(clip);
        }
        else
        {
            playbackTarget.clip = clip;
            playbackTarget.Play();
        }
    }

    private void Update()
    {
        if (audioClips.Count > 0 && !playbackTarget.isPlaying)
        {
            playbackTarget.clip = audioClips.Dequeue();
            playbackTarget.Play();
        }
    }
}
