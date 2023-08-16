using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EmotionTriggerController : MonoBehaviour
{
    [System.Serializable]
    private struct EmotionTriggerEntry
    {
        public string animationName;
        public KeyCode keyCode;
    }

    [System.Serializable]
    private struct EmotionGroup
    {
        public string category;
        public KeyCode keyCode;
        public EmotionTriggerEntry[] emotions;
    }

    private Animator animator;

    [SerializeField]
    private EmotionGroup[] emotionGroups;

    private Dictionary<string, string[]> emotionGroupLookup = new Dictionary<string, string[]>();

    [Header("Input Control")]
    [SerializeField]
    private bool enableKeyTriggers = true;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        foreach(var emotionGroup in emotionGroups)
        {
            string[] animations = new string[emotionGroup.emotions.Length];
            for(int i = 0; i <  animations.Length; i++)
            {
                animations[i] = emotionGroup.emotions[i].animationName;
            }
            emotionGroupLookup.Add(emotionGroup.category, animations);
        }
    }

    public void TriggerEmotion(string category)
    {
        if(emotionGroupLookup.TryGetValue(category, out var emotions))
        {
            TriggerEmotionFromGroup(category, emotions);
        }
        else
        {
            Debug.LogWarning($"[Emotion Trigger Controller] Emotion category {category} not found");
        }
    }

    private void Update()
    {
        if (enableKeyTriggers)
        {
            foreach(var emotionGroup in emotionGroups)
            {
                if (Input.GetKeyUp(emotionGroup.keyCode))
                {
                    TriggerEmotionFromGroup(emotionGroup.category, emotionGroup.emotions);
                    break;
                }

                bool foundEmotion = false;
                foreach(var emotion in emotionGroup.emotions)
                {
                    if (Input.GetKeyUp(emotion.keyCode))
                    {
                        animator.CrossFadeInFixedTime(emotion.animationName, 0.5f);
                        foundEmotion = true;
                        break;
                    }
                }

                if (foundEmotion) break;
            }
        }
    }

    private void TriggerEmotionFromGroup(string category, string[] emotions)
    {
        if (emotions.Length == 0)
        {
            Debug.LogWarning($"[Emotion Trigger Controller] Emotion category {category} has no emotions!");
            return;
        }
        int index = Random.Range(0, emotions.Length);
        animator.CrossFadeInFixedTime(emotions[index], 0.5f);
    }

    private void TriggerEmotionFromGroup(string category, EmotionTriggerEntry[] emotions)
    {
        if (emotions.Length == 0)
        {
            Debug.LogWarning($"[Emotion Trigger Controller] Emotion category {category} has no emotions!");
            return;
        }
        int index = Random.Range(0, emotions.Length);
        animator.CrossFadeInFixedTime(emotions[index].animationName, 0.5f);
    }
}
