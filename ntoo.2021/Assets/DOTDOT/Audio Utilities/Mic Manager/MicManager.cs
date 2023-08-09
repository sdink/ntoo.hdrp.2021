using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MicManager : MonoBehaviour
{

  public enum MicState { Idle, Monitoring, Listening }

  [Serializable]
  public class MicConfig
  {
    [Header("Volume Controls")]

    [Tooltip("Threshold for noise before NTOO starts listening")]
    [Range(0, 1f)]
    public float threshold;

    [Tooltip("Deadzone between noise threshold on and off")]
    [Range(0, 0.000004f)]
    public float deadZone;

    [Tooltip("Smoothing factor for threshold detection - lower values increase smoothing and latency.")]
    [Range(0.001f, 1f)]
    public float levelSmoothing;

    [Tooltip("Whether to use RMS for level calculations")]
    public bool useRms;

    [Header("Durations")]

    [Tooltip("Duration of main Mic loop, keep this short but longer than latency from smoothing factor")]
    [Range(0, 5)]
    public int loopDuration;

    [Tooltip("Duration of silence before NTOO starts talking")]
    [Range(0, 10)]
    public float silenceDuration;

    [Tooltip("Duration NTOO's active listening, used for understanding the bulk of the user's intended communication")]
    [Range(0, 60)]
    public float maxListeningDuration;

    [Tooltip("Index of the microphone to use")]
    public string mic;
  }

  [Header("Configuration")]
  [SerializeField]
  private string configFile = "MicConfig.json";

  [SerializeField]
  private MicConfig config = new MicConfig
  {
    threshold = 0.0001f,
    deadZone = 0.000002f,
    levelSmoothing = 0.05f,
    useRms = false,
    loopDuration = 1,
    silenceDuration = 2,
    maxListeningDuration = 30,
    mic = null
  };

  private float micVolumePeakAverage = 0;

  [SerializeField]
  [Tooltip("State to enter after recording a sample")]
  private MicState stateAfterSending = MicState.Idle;

  [Serializable] private class AudioEvent : UnityEvent<float[], int, int> { }
  [Header("Client Events")]
  [SerializeField] private AudioEvent OnReadyToSendAudio;

  [Serializable] private class NtooEvent : UnityEvent { }
  [Header("Ntoo Manager Events")]
  [SerializeField] private NtooEvent OnStoppedMonitoring;

  [Serializable] private class LevelEvent : UnityEvent<float> { }
  [Header("Monitoring Events")]
  [SerializeField] private LevelEvent OnMicLevelUpdated;
  [SerializeField] private LevelEvent OnThresholdLevelUpdated;
  
  private MicState state = MicState.Idle;

  public MicState State
  {
    get
    {
      return state;
    }

    set
    {
      if (value == state) return;

      if (value == MicState.Idle)
      {
        if (state == MicState.Listening)
        {
          // Clean up recording
          StopListening();
        }

        // Stop microphone
        StopMonitoring();
        state = value;
        Debug.Log("[Mic Manager] Mic now Idle");
      }
      else if (value == MicState.Listening)
      {
        if (state == MicState.Idle)
        {
          // Start microphone and init buffers
          if (!StartMonitoring()) return;
        }

        // Start recording
        StartListening();
        state = value;
        Debug.Log("[Mic Manager] Started Listening");
      }
      else if (value == MicState.Monitoring)
      {
        if (state == MicState.Idle)
        {
          // Start Microphone and Monitor
          if (!StartMonitoring()) return;
        }
        else if (state == MicState.Listening)
        {
          // Clean up Listening
          StopListening();
        }

        state = value;
        Debug.Log("[Mic Manager] Started Monitoring");
      }
      else
      {
        Debug.LogWarning("[Mic Manager] Unrecognised state transition");
      }
    }
  }

  public float MicThreshold 
  { 
    get { return config.threshold; } 
    set
    {
      if (value == config.threshold) return;
      config.threshold = value;
      SaveConfig();
    }
  }

  public string SelectedMic { get { return config.mic; } }

  // Non-exposed variables

  // Microphone settings
  private string activeMicName;
  private int micDeviceFrequency;
  private int micLastSamplePos = 0;
  private bool volumeHitThreshold = false;
  private float lastVolumeThresholdChange = 0;

  // Active listening
  private float[] activeListeningBuffer;
  private int activeListeningBufferOffset = 0;

  private AudioClip micRecording;
  private float[] preRecordLoop;
  private bool listeningLoopPassedBuffer = false;

  private string configFilePath
  {
    get
    {
      return Path.Combine(Application.persistentDataPath, configFile);
    }
  }

  void Start()
  {
    if (File.Exists(configFilePath))
    {
      try
      {
        string configJson = File.ReadAllText(configFilePath);
        JsonUtility.FromJsonOverwrite(configJson, config);
      }
      catch (Exception e)
      {
        Debug.LogError("[Mic Manager] Error reading config from file: " + e.Message);
      }

      OnThresholdLevelUpdated.Invoke(config.threshold);
    } 
    else
    {
      SaveConfig();
    }

    // Check if there is at least one microphone connected.
    if (Microphone.devices.Length <= 0)
    {
      Debug.Log("Warning: No Microphone Detected!");
    }
    else // At least one microphone is available.
    {
      InitialiseMicrophone();
      InitialiseBuffers();
    }

    OnMicLevelUpdated.Invoke(0); // initialise with 0 mic level for monitors
  }

  private void SaveConfig()
  {
    string configJson = JsonUtility.ToJson(config);
    try
    {
      File.WriteAllText(configFilePath, configJson);
    }
    catch (Exception e)
    {
      Debug.LogError("[Mic Manager] Error writing config to file: " + e.Message);
    }
  }

  public void SetStateIdle()
  {
    State = MicState.Idle;
  }

  public void SetStateMonitoring()
  {
    State = MicState.Monitoring;
  }

  public void SetStateListening()
  {
    State = MicState.Listening;
  }

  public void CancelSend()
  {
    if (State != MicState.Idle)
    {
      State = MicState.Monitoring;
    }
  }

  /// <summary>
  /// Initialises the device frequency and device name.
  /// </summary>
  /// <param name="fallbackFrequency"></param>
  private void InitialiseMicrophone(int fallbackFrequency = 44100)
  {
    if (string.IsNullOrEmpty(config.mic))
    {
      config.mic = Microphone.devices[0];
      SaveConfig();
    }

    Debug.Log("[MicManager] Initialising microphone " + config.mic);
    ConfigureMicrophone(fallbackFrequency);
  }

  /// <summary>
  /// Select the given microphone device and, if currently recording,
  /// switch recording to the new device.
  /// </summary>
  /// <param name="micIndex"></param>
  public void SetMicrophone(Int32 micIndex)
  {
    //Debug.Log($"[MicManager] Attempting to set microphone to index {micIndex}...");
    if (micIndex > 0 && micIndex < Microphone.devices.Length)
    {
      string micDeviceName = Microphone.devices[micIndex];
      if (config.mic != micDeviceName)
      {
        Debug.Log($"[MicManager] Set Microphone Device to {micDeviceName}.");
        config.mic = micDeviceName;
        SaveConfig();

        if (!string.IsNullOrEmpty(activeMicName) && State != MicState.Idle)
        {
          MicState currentState = State;
          State = MicState.Idle;
          ConfigureMicrophone();
          State = currentState;
        }
        else
        {
          ConfigureMicrophone();
        }
      }
      //else Debug.Log($"[MicManager] ...But couldn't because it is identical to the current index of {micSelectedIndex}.");
    }
    //else Debug.Log($"[MicManager] ...But couldn't because it does not fall within the range of 0 and {Microphone.devices.Length}.");
  }

  private void ConfigureMicrophone(int fallbackFrequency = 44100)
  {

    // ensure microphone is valid
    if (Microphone.devices.Contains(config.mic))
    {
      activeMicName = config.mic;
    }
    else
    {
      Debug.LogWarning("[Mic Manager] Configured device not found - falling back to default");
      activeMicName = Microphone.devices[0];
    }

    // Poll the device's frequency capabilities.
    int _minFreq, _maxFreq;
    Microphone.GetDeviceCaps(activeMicName, out _minFreq, out _maxFreq);

    // According to the documentation, if minFreq and maxFreq are zero,
    // the microphone supports any frequency, meaning we can use the
    // fallback frequency (default: 44100).
    if (_minFreq == 0 && _maxFreq == 0) _maxFreq = fallbackFrequency;
    micDeviceFrequency = _maxFreq;
    Debug.Log($"[Mic Manager] Using ${activeMicName} with min frequency of {_minFreq} and max frequency of {_maxFreq}.");
  }

  /// <summary>
  /// Initialises passive, active, and volumeDetection buffers and corresponding capacities.
  /// </summary>
  public void InitialiseBuffers()
  {
    // Active Listening Buffer
    int _activeListeningBufferCapacity = (int)Math.Ceiling(micDeviceFrequency * config.maxListeningDuration);
    Debug.Log($"[MicManager] Creating new Active Listening Buffer with capacity {_activeListeningBufferCapacity}.");
    activeListeningBuffer = new float[_activeListeningBufferCapacity];
  }

  /// <summary>
  /// If volume hit threshold, do active listening. Otherwise, do passive listening.
  /// </summary>
  private void FixedUpdate()
  {
    if (State == MicState.Idle) return;

    // Read data from microphone clip and store in micBuffer.
    float[] latestData = ReadMicrophoneData();

    if (State == MicState.Listening && latestData != null)
    {
      if (activeListeningBufferOffset + latestData.Length >= activeListeningBuffer.Length)
      {
        int remainingBuffer = activeListeningBuffer.Length - activeListeningBufferOffset;
        for(int i = 0; i < remainingBuffer; i++)
        {
          activeListeningBuffer[activeListeningBufferOffset + i] = latestData[i];
        }
        Debug.LogWarning($"[Mic Manager] Sending audio due to buffer overflow (this can indicate someone was still talking and we cut them off). Listening duration: {Time.time - lastVolumeThresholdChange}. Max Duration: {config.maxListeningDuration}.");
        // The buffer is full, send what we have.
        // Note: This error should never occur as we're handling it below in TestInputVolume().
        SendAudio();
      }
      else
      {
        latestData.CopyTo(activeListeningBuffer, activeListeningBufferOffset);
        activeListeningBufferOffset += latestData.Length;
      }
    }

    // Test micBuffer volume against threshold.
    TestInputVolume(latestData);
  }

  /// <summary>
  /// Get latest microphone data.
  /// </summary>
  private float[] ReadMicrophoneData()
  {
    // Read the number of samples remaining in the microphone's clip.
    int micSamplePos = Microphone.GetPosition(activeMicName);
    int samplesToRead = micSamplePos - micLastSamplePos;

    // Skip if we are up to date
    if (samplesToRead == 0) return null;

    // Check for wraparound
    if (samplesToRead < 0)
    {
      samplesToRead += micRecording.samples;
      listeningLoopPassedBuffer = true;
    }

    // Read data (note GetData will automatically wrap around)
    float[] newSamples = new float[samplesToRead];
    micRecording.GetData(newSamples, micLastSamplePos);

    // Update last sample pos for next time.
    micLastSamplePos = micSamplePos;

    return newSamples;
  }


  private float minLevel = 0;
  private float maxLevel = 0;
  private float previousSample = 0;
  private float previousDelta = 0;
  private bool newMinLevel = false;
  private bool newMaxLevel = false;

  private float peakToPeakLevel = 0;
  private float rmsLevel = 0;

  private float testLevel = 0;

  /// <summary>
  /// Test input volume against threshold.
  /// </summary>
  private void TestInputVolume(float[] input)
  {
    if (State == MicState.Idle || input == null) return;

    if (config.useRms)
    {
      // Calculate RMS level
      float sum = 0;
      foreach (float sample in input)
      {
        sum += sample;
      }
      float avg = sum / input.Length;
      float sumMeanSq = 0;
      foreach (float sample in input)
      {
        sumMeanSq += Mathf.Pow(sample - avg, 2);
      }
      float avgMeanSq = sumMeanSq / input.Length;
      rmsLevel = Mathf.Pow(avgMeanSq, 0.5f);
      testLevel = rmsLevel;
    }
    else
    {
      // Calculate averaged peak-to-peak levels
      foreach (float sample in input)
      {
        float delta = sample - previousSample;
        if (delta == 0)
        {
          if (previousDelta < 0)
          {
            minLevel = sample;
            newMinLevel = true;
          }
          else
          {
            maxLevel = sample;
            newMaxLevel = true;
          }
        }
        else if (delta > 0 && previousDelta < 0)
        {
          minLevel = previousSample;
          newMinLevel = true;
        }
        else if (delta < 0 && previousDelta > 0)
        {
          maxLevel = previousSample;
          newMaxLevel = true;
        }

        if (newMinLevel && newMaxLevel)
        {
          peakToPeakLevel = maxLevel - minLevel;
          newMinLevel = false;
          newMaxLevel = false;
        }

        float volumeDelta = peakToPeakLevel - micVolumePeakAverage;
        micVolumePeakAverage += volumeDelta * config.levelSmoothing;
        previousDelta = delta;
        previousSample = sample;
      }

      testLevel = micVolumePeakAverage;
    }

    OnMicLevelUpdated.Invoke(testLevel);

    // Test against threshold.
    if (testLevel > config.threshold)
    {
      if (!volumeHitThreshold)
      {
        lastVolumeThresholdChange = Time.time;
        volumeHitThreshold = true;
        State = MicState.Listening;
      }
    }
    else if (testLevel < config.threshold - config.deadZone)
    {
      if (volumeHitThreshold)
      {
        volumeHitThreshold = false;
        lastVolumeThresholdChange = Time.time;
      }

      if (State == MicState.Listening && Time.time - lastVolumeThresholdChange > config.silenceDuration)
      {
        // Detected pause - send audio
        Debug.Log($"Detected pause. Thinking...");
        volumeHitThreshold = false;
        SendAudio();
      }
    }

    if (State == MicState.Listening && Time.time - lastVolumeThresholdChange >= config.maxListeningDuration)
    {
      // Reached maximum length, send audio
      SendAudio();
    }
  }

  /// <summary>
  /// Bundles up the PreRecord Loop Buffer with the Active Listening Buffer and sends to the client
  /// manager for streaming to the server.
  /// </summary>
  private void SendAudio()
  {
    if (preRecordLoop == null || activeListeningBuffer == null || activeListeningBufferOffset == 0)
    {
      // no data to send
      State = MicState.Monitoring;
      return;
    }

    Debug.Log($"[Mic Manager] Sending audio with preRecordLoop length of {preRecordLoop.Length} and recorded length of {activeListeningBufferOffset}");

    // 1. Combine circular buffer with extended buffer then send to server.
    // Note: Size means total non-default entries - see `for loop` below.
    int _totalCapacity = preRecordLoop.Length + activeListeningBufferOffset;

    // 2. Create a new array holding the contents of the passive and (all meaningful values of) the active buffer.
    float[] combinedArray = new float[_totalCapacity];
    preRecordLoop.CopyTo(combinedArray, 0); // This can be further optimised.
    for (int i = 0; i < activeListeningBufferOffset; i++)
    {
      combinedArray[preRecordLoop.Length + i] = activeListeningBuffer[i];
    }

    // 3. Send data to client manager to stream to server.
    OnReadyToSendAudio.Invoke(combinedArray, 1, micDeviceFrequency);

    // 4. Transition to post-audio send state
    State = stateAfterSending;
  }

  /// <summary>
  /// Begin reading input from the selected microphone device.
  /// </summary>
  private bool StartMonitoring()
  {
    if (string.IsNullOrEmpty(activeMicName)) return false;
    Debug.Log("Starting microphone: " + activeMicName);
    // reset monitoring variables
    volumeHitThreshold = false;
    lastVolumeThresholdChange = Time.time;
    Debug.Log("[Mic Manager] Resetting loop buffer trigger");
    listeningLoopPassedBuffer = false;
    micLastSamplePos = 0;

    // start microphone
    micRecording = Microphone.Start(activeMicName, true, config.loopDuration, micDeviceFrequency);
    return micRecording != null;
  }

  /// <summary>
  /// Stop reading input into the selected microphone device.
  /// </summary>
  private void StopMonitoring()
  {
    if (string.IsNullOrEmpty(activeMicName)) return;

    // Stop the recording.
    Microphone.End(activeMicName); // Stop the audio recording

    OnStoppedMonitoring.Invoke();
    OnMicLevelUpdated.Invoke(0); // update monitor with silent mic input
  }

  private void StartListening()
  {
    activeListeningBufferOffset = 0;
    // store current contents of clip buffer for appending
    int micPosition = Microphone.GetPosition(activeMicName);
    Debug.Log($"[Mic Manager] Started listening - micPos: {micPosition}, full buffer: {listeningLoopPassedBuffer}");
    if (listeningLoopPassedBuffer)
    {
      preRecordLoop = new float[micRecording.samples];
      if (micPosition == micRecording.samples - 1)
      {
        micRecording.GetData(preRecordLoop, 0);
      }
      else
      {
        // increment by 1 to start from the oldest value (the one that will
        // next be overridden by the mic)
        micRecording.GetData(preRecordLoop, micPosition + 1);
      }
    }
    else
    {
      preRecordLoop = new float[micPosition];
      micRecording.GetData(preRecordLoop, 0);
    }

    Debug.Log($"[Mic Manager] Initialised Pre Record Loop with length {preRecordLoop.Length}");
  }

  private void StopListening()
  {
    preRecordLoop = null;
  }

  void OnDisable()
  {
    State = MicState.Idle;
  }

  void OnDestroy()
  {
    State = MicState.Idle;
    activeMicName = null;
  }
}