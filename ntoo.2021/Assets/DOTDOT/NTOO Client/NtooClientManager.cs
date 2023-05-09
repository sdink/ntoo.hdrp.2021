using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using Ntoo.Wave;

public class NtooClientManager : MonoBehaviour
{
    [SerializeField] private int chunkSize = (int) Math.Pow(2, 16);
    [SerializeField] private bool useChunks = true;
    [Serializable] public class MicEvent : UnityEvent<AudioClip> { }
    [SerializeField] public MicEvent PlayAudioClip = null;
    [Serializable] private class NetworkEventText : UnityEvent<string> { }
    [SerializeField] private NetworkEventText SendTextMessage = null;
    [Serializable] private class NetworkEventData : UnityEvent<byte[]> { }
    [SerializeField] private NetworkEventData SendBinaryData = null;
    public void OnReceivedAudio(byte[] audioData)
    {
        int channels = 1;
        int frequency = 22050;
        AudioClip clip = WaveUtility.WaveDataToClip(audioData, channels, frequency);
        PlayAudioClip.Invoke(clip);
    }
    public void StreamAudio(AudioClip audioClip)
    {
        Debug.Log("CHANNELS: "+audioClip.channels);

        // Get byte[] data from audioClip.
        byte[] audioBytes = WaveUtility.ClipToWaveData(audioClip);

        // Tell server to expect stream.
        int numChunks = Mathf.Clamp(audioBytes.Length / chunkSize, 1, audioBytes.Length);
        Debug.Log("Signalling the server to start streaming.");
        SendTextMessage.Invoke($"START:{numChunks}:{audioClip.frequency}:{audioClip.channels}");

        // Chunkify
        if (useChunks == false || audioBytes.Length < chunkSize)
        {
            Debug.Log($"useChunks: {useChunks}");
            // No chunking necessary - send entire audio data.
            string logStr = BitConverter.ToString(audioBytes);
            Debug.Log($"Sending entire audio : {logStr}");
            SendBinaryData.Invoke(audioBytes);
        }
        else
        {
            // Break into {chunkSize} chunks and send consecutively.
            byte[] chunk = new byte[chunkSize];
            for (int i=0; i<audioBytes.Length / chunkSize; i++)
            {
                Buffer.BlockCopy(audioBytes, i*chunkSize, chunk, 0, chunkSize);
                string logStr = BitConverter.ToString(chunk);
                Debug.Log($"Sending chunk {i}: {logStr}");
                SendBinaryData.Invoke(chunk);
            }
            Debug.Log($"Finished sending {audioBytes.Length/chunkSize} chunks.");
        }

        // Tell server to stop expecting chunks and save what it's received.
        Debug.Log("Signalling the server to stop streaming.");
        SendTextMessage.Invoke("STOP");
    }
}