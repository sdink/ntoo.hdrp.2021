using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CaptionController : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<string> OnUpdateCaptionText;

    [SerializeField]
    private UnityEvent<bool> OnShowCaption;

    [SerializeField]
    private AudioSource audioPlayback;

    private bool captionQueued = false;
    private bool showingCaption = false;

    public void UpdateCaption(string caption)
    {
       OnUpdateCaptionText.Invoke(caption);
       captionQueued = true;
    }

    private void Update()
    {
        if (showingCaption)
        {
            if (!audioPlayback.isPlaying)
            {
                OnShowCaption.Invoke(false);
                captionQueued = false;
                showingCaption = false;
            }
        }
        else if (audioPlayback.isPlaying && captionQueued)
        {
            showingCaption = true;
            OnShowCaption.Invoke(true);
        }
    }
}
