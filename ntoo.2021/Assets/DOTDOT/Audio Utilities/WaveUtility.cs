using UnityEngine;
using System.IO;
using System;

namespace Ntoo.Wave
{
  public class WaveUtility : MonoBehaviour
  {
    /// <summary>
    /// Gets the peak volume within the last period of time equal to volumeDetectionMaxDuration.
    /// </summary>
    /// <returns>(float) Maximum peak volume</returns>
    public static float FindWavePeak(in float[] micBuffer)
    {
      // Find a peak in the last window of samples.
      float _maxPeak = 0;
      foreach (float _value in micBuffer)
      {
        float _wavePeak = _value * _value;
        if (_maxPeak < _wavePeak)
        {
          _maxPeak = _wavePeak;
        }
      }
      return _maxPeak;
    }

    private static readonly int FLOAT_SIZE = 4;

    // Audio Clip to Wave Data
    public static byte[] ClipToWaveData(AudioClip clip)
    {
      float[] samples = new float[clip.samples];
      clip.GetData(samples, 0);
      return FloatsToInt16ByteArray(samples);
    }
    //*/
    private static byte[] FloatsToBytes(float[] floatArray)
    {
      // This can be used to test a straight float to byte conversion.
      //Debug.Log("Converting floats to bytes...");
      byte[] byteArray = new byte[floatArray.Length * FLOAT_SIZE];
      Buffer.BlockCopy(floatArray, 0, byteArray, 0, floatArray.Length);
      return byteArray;
    }
    public static byte[] FloatsToInt16ByteArray(float[] data)
    {
      MemoryStream dataStream = new MemoryStream();
      WriteFloatArrayAsBytes(dataStream, data);
      byte[] bytes = dataStream.ToArray();

      // Validate converted bytes
      Debug.AssertFormat(data.Length * 2 == bytes.Length, $"Unexpected float[] to Int16 to byte[] size: {data.Length * 2} == {bytes.Length}");
      dataStream.Dispose();
      return bytes;
    }

    public static byte[] FloatsToWav(float[] data, int channels, int frequency)
    {
      MemoryStream dataStream = new MemoryStream();
      WriteWav(dataStream, data, (ushort)channels, (uint)frequency);
      byte[] bytes = dataStream.ToArray();
      dataStream.Dispose();
      return bytes;
    }

    // Wave Data to Audio Clip
    public static AudioClip WaveDataToClip(byte[] audioData, int channels, int frequency)
    {
      float[] audioFloats = BytesToFloats(audioData);
      AudioClip clip = AudioClip.Create("OutputAudio", audioFloats.Length, channels, frequency, false);
      // Debug.Log(string.Join(", ", audioFloats));
      clip.SetData(audioFloats, 0);
      return clip;
    }
    private static float[] BytesToFloats(byte[] byteArray)
    {
      // This can be used to test a straight byte to float conversion.
      //Debug.Log("Converting bytes to floats...");
      float[] floatArray = new float[byteArray.Length / FLOAT_SIZE];
      Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
      return floatArray;
    }
    private static float[] Int16ByteArrayToFloats(byte[] source)
    {
      int x = sizeof(Int16); // block size = 2
      int convertedSize = source.Length / x;
      float[] data = new float[convertedSize];
      Int16 maxValue = Int16.MaxValue;
      int offset = 0;

      for (int i = 0; i < convertedSize; i++)
      {
        offset = i * x;
        data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
      }

      Debug.AssertFormat(data.Length == convertedSize, $"AudioClip .wav data is wrong size: {data.Length} == {convertedSize}");

      return data;
    }
    private static float[] SignedToUnsigned(byte[] source)
    {
      float[] data = new float[source.Length];
      for (int i = 0; i < source.Length; i++)
      {
        data[i] = (source[i] + 1) / 2;
      }

      return data;
    }

    // Function for exporting raw Unity audio float data to a wav file
    // Sourced from https://stackoverflow.com/questions/50864146/create-a-wav-file-from-unity-audioclip 
    // Untested with multi-channel
    public static void SaveAudioToFile(float[] data, ushort channels, uint frequency, string file = "Recording.wav")
    {
      var path = Path.Combine(Application.persistentDataPath, file);
      using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
      {
        WriteWav(stream, data, channels, frequency);
      }
    }

    private static void WriteWav(Stream stream, float[] data, ushort channels, uint frequency)
    {
      // The following values are based on http://soundfile.sapp.org/doc/WaveFormat/
      var bitsPerSample = (ushort)16;
      var chunkID = "RIFF";
      var format = "WAVE";
      var subChunk1ID = "fmt ";
      var subChunk1Size = (uint)16;
      var audioFormat = (ushort)1;
      var numChannels = channels;
      var sampleRate = frequency;
      var byteRate = (uint)(sampleRate * channels * bitsPerSample / 8);  // SampleRate * NumChannels * BitsPerSample/8
      var blockAlign = (ushort)(numChannels * bitsPerSample / 8); // NumChannels * BitsPerSample/8
      var subChunk2ID = "data";
      var subChunk2Size = (uint)(data.Length * channels * bitsPerSample / 8); // NumSamples * NumChannels * BitsPerSample/8
      var chunkSize = (uint)(36 + subChunk2Size); // 36 + SubChunk2Size
                                                  // Start writing the file.
      WriteString(stream, chunkID);
      WriteInteger(stream, chunkSize);
      WriteString(stream, format);
      WriteString(stream, subChunk1ID);
      WriteInteger(stream, subChunk1Size);
      WriteShort(stream, audioFormat);
      WriteShort(stream, numChannels);
      WriteInteger(stream, sampleRate);
      WriteInteger(stream, byteRate);
      WriteShort(stream, blockAlign);
      WriteShort(stream, bitsPerSample);
      WriteString(stream, subChunk2ID);
      WriteInteger(stream, subChunk2Size);
      WriteFloatArrayAsBytes(stream, data);
    }

    private static void WriteFloatArrayAsBytes(Stream stream, float[] data)
    {
      foreach (var sample in data)
      {
        // De-normalize the samples to 16 bits.
        var deNormalizedSample = (short)0;
        if (sample > 0)
        {
          var temp = sample * short.MaxValue;
          if (temp > short.MaxValue)
            temp = short.MaxValue;
          deNormalizedSample = (short)temp;
        }
        if (sample < 0)
        {
          var temp = sample * (-short.MinValue);
          if (temp < short.MinValue)
            temp = short.MinValue;
          deNormalizedSample = (short)temp;
        }
        WriteShort(stream, (ushort)deNormalizedSample);
      }
    }

    private static void WriteString(Stream stream, string value)
    {
      foreach (var character in value)
        stream.WriteByte((byte)character);
    }

    private static void WriteInteger(Stream stream, uint value)
    {
      stream.WriteByte((byte)(value & 0xFF));
      stream.WriteByte((byte)((value >> 8) & 0xFF));
      stream.WriteByte((byte)((value >> 16) & 0xFF));
      stream.WriteByte((byte)((value >> 24) & 0xFF));
    }

    private static void WriteShort(Stream stream, ushort value)
    {
      stream.WriteByte((byte)(value & 0xFF));
      stream.WriteByte((byte)((value >> 8) & 0xFF));
    }
  }
}