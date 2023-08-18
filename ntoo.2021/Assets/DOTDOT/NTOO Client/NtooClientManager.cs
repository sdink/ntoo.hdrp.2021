using System;
using UnityEngine;
using UnityEngine.Events;
using Ntoo.Wave;

public class NtooClientManager : MonoBehaviour
{
  [SerializeField] private int chunkSize = (int)Math.Pow(2, 16);
  [SerializeField] private bool useChunks = true;
  [Serializable] public class SpeakerEvent : UnityEvent<AudioClip> { }
  [Header("Speaker Events")]
  [SerializeField] public SpeakerEvent OnReceivedAudioClip;
  [SerializeField] public UnityEvent<string> OnReceivedSentiment;
  public UnityEvent OnReceivedEmptyClip;

  [Serializable] private class NetworkEventText : UnityEvent<string> { }
  [Header("Network Manager Events")]
  [Tooltip("Assign this to your network manager's text transmission method.")][SerializeField] private NetworkEventText OnSendTextMessage = null;
  [Serializable] private class NetworkEventData : UnityEvent<byte[]> { }
  [Tooltip("Assign this to your network manager's binary data transmission method.")][SerializeField] private NetworkEventData OnSendBinaryData = null;

  public void ReceiveAudio(byte[] audioData)
  {
    if (audioData == null || audioData.Length == 0)
    {
      OnReceivedEmptyClip.Invoke();
    }
    else
    {
      int channels = 1;
      int frequency = 22050;
      AudioClip clip = WaveUtility.WaveDataToClip(audioData, channels, frequency);
      OnReceivedAudioClip.Invoke(clip);
    }
  }

  public void ReceiveMessage(string message)
  {
    Debug.Log("[Ntoo Client Manager] Received message: " + message);
    if (message == "Cancel")
    {
      Debug.Log("[Ntoo Client Manager] Triggering empty clip received");
      OnReceivedEmptyClip.Invoke();
    }
    else if (message.StartsWith("Sentiment:"))
    {
      OnReceivedSentiment.Invoke(message.Substring(9));
    }
  }

  /// <summary>
  /// Initiates a conversation with NTOO by telling the server to send back the given string as spoken word.
  /// </summary>
  /// <param name="response"></param>
  public void SendCannedResponse(string response)
  {
    // Note: This will inform the server that the conversation that follows is independent from the previous conversation.
    Debug.Log($"[ClientManager] Initiating a new conversation with: \"{response}\".");
    OnSendTextMessage.Invoke($"RESPOND:{response}");
  }
  public void StreamAudio(float[] audioBuffer, int channels, int frequency)
  {
    // Get byte[] data from audioClip.
    //byte[] audioBytes = WaveUtility.FloatsToInt16ByteArray(audioBuffer);
    byte[] wavBytes = WaveUtility.FloatsToWav(audioBuffer, channels, frequency);

#if NTOO_CLIENT_DEBUG
    WaveUtility.SaveAudioToFile(audioBuffer, (ushort)channels, (uint)frequency, "NtooInputRecording.wav");
#endif

    // Tell server to expect stream.
    int numChunks = Mathf.Clamp(wavBytes.Length / chunkSize, 1, wavBytes.Length);
    //Debug.Log("[NTOO Client] Signalling the server to start streaming.");
    OnSendTextMessage.Invoke($"START:WAV:{numChunks}");

    // Chunkify
    if (useChunks == false || wavBytes.Length < chunkSize)
    {
      // No chunking necessary - send entire audio data.
      Debug.Log("[NTOO Client] Sending audio as single chunk");
      OnSendBinaryData.Invoke(wavBytes);
    }
    else
    {
      // Break into {chunkSize} chunks and send consecutively.
      byte[] chunk = new byte[chunkSize];
      int chunkCount = wavBytes.Length / chunkSize;
      for (int i = 0; i < chunkCount; i++)
      {
        Buffer.BlockCopy(wavBytes, i * chunkSize, chunk, 0, chunkSize);
        Debug.Log($"[NTOO Client] Sending audio chunk {i}");
        OnSendBinaryData.Invoke(chunk);
      }

      if (wavBytes.Length > chunkCount * chunkSize)
      {
        int remainder = wavBytes.Length - chunkCount * chunkSize;
        byte[] finalChunk = new byte[remainder];
        Buffer.BlockCopy(wavBytes, chunkCount * chunkSize, finalChunk, 0, remainder);
        Debug.Log($"[NTOO Client] Sending final audio chunk");
        OnSendBinaryData.Invoke(finalChunk);
      }

      Debug.Log($"Finished sending chunks.");
    }

    // Tell server to stop expecting chunks and save what it's received.
    //Debug.Log("[NTOO Client] Signalling the server to stop streaming.");
    OnSendTextMessage.Invoke("STOP");
  }
}