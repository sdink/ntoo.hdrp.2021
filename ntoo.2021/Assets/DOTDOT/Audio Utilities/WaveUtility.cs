using UnityEngine;
using System.Text;
using System.IO;
using System;

namespace Ntoo.Wave
{
    public class WaveUtility : MonoBehaviour
    {
        private static readonly int FLOAT_SIZE = 4;

        // Audio Clip to Wave Data
        public static byte[] ClipToWaveData(AudioClip clip)
        {
            var samples = new float[clip.samples];
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
        public static byte[] FloatsToInt16ByteArray (float[] data)
        {
            MemoryStream dataStream = new MemoryStream();

            int x = sizeof(Int16);
            Debug.Log("sizeof Int16: "+x);

            Int16 maxValue = Int16.MaxValue;

            for (int i=0; i < data.Length; i++)
            {
                short itemInt16 = Convert.ToInt16(data[i] * maxValue);
                byte[] itemBytes = BitConverter.GetBytes(itemInt16);
                dataStream.Write(itemBytes, 0, x);
            }

            byte[] bytes = dataStream.ToArray();

            // Validate converted bytes
            Debug.AssertFormat(data.Length * x == bytes.Length, $"Unexpected float[] to Int16 to byte[] size: {data.Length * x} == {bytes.Length}");

            dataStream.Dispose();

            return bytes;
        }
        
        // Wave Data to Audio Clip
        public static AudioClip WaveDataToClip(byte[] audioData, int channels, int frequency)
        {
            AudioClip clip = AudioClip.Create("OutputAudio", audioData.Length, channels, frequency, false);
            float[] audioFloats = BytesToFloats(audioData);
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
        private static float[] Int16ByteArrayToFloats (byte[] source)
        {
            int x = sizeof(Int16); // block size = 2
            int convertedSize = source.Length / x;
            float[] data = new float[convertedSize];
            Int16 maxValue = Int16.MaxValue;
            int offset = 0;

            for (int i=0; i < convertedSize; i++)
            {
                offset = i * x;
                data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
            }

            Debug.AssertFormat(data.Length == convertedSize, $"AudioClip .wav data is wrong size: {data.Length} == {convertedSize}");

            return data;
        }
        private static float[] SignedToUnsigned (byte[] source)
        {
            float[] data = new float[source.Length];
            for (int i=0; i < source.Length; i++)
            {
                data[i] = (source[i] + 1) / 2;
            }

            return data;
        }
    }
}