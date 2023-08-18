using UnityEngine;
using System;
using System.IO;
using UnityEngine.Events;

public class NtooManager : MonoBehaviour
{
    [Serializable]
    public struct Greeting
    {
        public string message;
        public string sentiment;
    }

  [Serializable]
  public class NtooManagerConfig
  {
    public Greeting[] greetings;
    public float heightOffset;
  }

  [Serializable] private class ClientEventText : UnityEvent<string> { }
  [Serializable] private class ClientEventAudio : UnityEvent<AudioClip> { }
  [Serializable] private class SpeakerEvent : UnityEvent { }
  [Serializable] private class MicEvent : UnityEvent { }
  [Serializable] private class MicSetIndexEvent : UnityEvent<int> { }

  [Header("Client Events")]
  [SerializeField] private ClientEventText OnTriggerConversation;
  [SerializeField] private ClientEventAudio OnStartSpeaking;
  [SerializeField] private UnityEvent<string> OnTriggerSentiment;
  [SerializeField] private UnityEvent<string> OnTriggerEmotion;
  [Header("Speaker Events")]
  [SerializeField] private SpeakerEvent OnStopSpeaking;
  [Header("Mic Events")]
  [SerializeField] private MicEvent OnStartListening;
  [SerializeField] private MicEvent OnStopListening;

  [Header("Configurable")]
  [SerializeField] private string configFile = "NtooConfig.json";
  [SerializeField] private NtooManagerConfig config;

  public Greeting[] Greetings
  {
    get
    {
      return config.greetings;
    }
  }

  private bool userPresent = false;
  public bool UserPresent
  {
    get { return userPresent; }
    set
    {
      if (value == userPresent) return;

      userPresent = value;
      if (userPresent)
      {
        BeginNtooRoutine();
      }
      else
      {
        StopNtooRoutine();
      }
    }
  }

  public enum States
  {
    Stopped,
    Speaking,
    Listening
  }

  public States State
  {
    get
    {
      return state;
    }
  }

  public string Sentiment { get; set; } = "Neutral";

  private States state = States.Stopped;

  private void Start()
  {
    string configFilePath = Path.Combine(Application.persistentDataPath, configFile);
    if (File.Exists(configFilePath))
    {
      try
      {
        string configJson = File.ReadAllText(configFilePath);
        JsonUtility.FromJsonOverwrite(configJson, config);
      }
      catch (Exception e)
      {
        Debug.LogError("[NTOO Manager] Error reading config from file: " + e.Message);
      }
    }
    else
    {
      string configJson = JsonUtility.ToJson(config);
      try
      {
        File.WriteAllText(configFilePath, configJson);
      }
      catch (Exception e)
      {
        Debug.LogError("[NTOO Manager] Error writing config to file: " + e.Message);
      }
    }

    transform.localPosition = new Vector3(0, config.heightOffset, 0);
  }

  public void BeginNtooRoutine()
  {
    TriggerConversationLoop();
  }

  public void StopNtooRoutine()
  {
    switch (state)
    {
      case States.Speaking:
        OnStopSpeaking.Invoke();
        break;

      case States.Listening:
        OnStopListening.Invoke();
        break;
    }
    state = States.Stopped;
  }

  /// <summary>
  /// Trigger a new conversation loop with a random greeting.
  /// </summary>
  public void TriggerConversationLoop()
  {
    int _index = (int)(UnityEngine.Random.value * Greetings.Length);
    TriggerConversationLoop(_index);
  }

  /// <summary>
  /// Trigger a new conversation loop with a given greeting index.
  /// </summary>
  /// <param name="index"></param>
  public void TriggerConversationLoop(int index)
  {
    if (state != States.Stopped)
    {
      Debug.Log("Warning: Attempting to trigger a new conversation loop while one is already in progress!");
      return;
    }
    Greeting _randomResponse = Greetings[index];
    Sentiment = _randomResponse.sentiment;
    OnTriggerConversation.Invoke(_randomResponse.message);
  }

  public void EnterSpeakingState(AudioClip audioToSpeak)
  {
    if (state == States.Speaking)
    {
      Debug.Log("Warning: Attempting to enter speaking state while already speaking.");
      return;
    }
    if (Sentiment.StartsWith("E:"))
    {
        OnTriggerEmotion.Invoke(Sentiment.Substring(2));
    }
    else
    {
        OnTriggerSentiment.Invoke(Sentiment);
    }
    
    OnStartSpeaking.Invoke(audioToSpeak);
    state = States.Speaking;
  }

  public void EnterListeningState()
  {
    if (state == States.Listening)
    {
      Debug.Log("Warning: Attempting to enter listening state while already listening.");
      return;
    }
    else if (!userPresent)
    {
      Debug.Log("No users present - returning to idle");
      StopNtooRoutine();
    }
    else
    {
      OnStartListening.Invoke();
      state = States.Listening;
    }
  }
}
