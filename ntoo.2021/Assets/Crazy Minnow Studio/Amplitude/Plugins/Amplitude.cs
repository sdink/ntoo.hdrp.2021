using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace CrazyMinnow.AmplitudeWebGL
{
    [AddComponentMenu("Crazy Minnow Studio/Amplitude/Amplitude")]
    public class Amplitude : MonoBehaviour
    {
        public AudioSource audioSource;
        public int sampleSize = 64;
        public bool absoluteValues = true;
        [Range(0f, 1f)]
		public float boost = 0.2f;
        public float freqBoost;
        public float average;
        public float max;
        public float[] sample;

        [HideInInspector] public enum DataType { Amplitude, Frequency}
        [HideInInspector] public DataType dataType = DataType.Amplitude;
        [HideInInspector] public string[] sampleSizeNames = new string[] { "64", "128", "256", "512", "1024", "2048" };
        [HideInInspector] public int[] sampleSizeVals = new int[] { 64, 128, 256, 512, 1024, 2048 };
        [HideInInspector] private float avgTotal = 0;
        [HideInInspector] private float maxTotal = 0;
        [HideInInspector] private const float boostBase = 1f;
        [HideInInspector] private bool prevPlayState;
        [HideInInspector] private bool run;
        [HideInInspector] private float playOnAwakeDelay = 0.1f;
        [HideInInspector] private float sign = 0;
        [HideInInspector] private float tempVal = 0;
        [HideInInspector] private const float freqBoostMin = 8.5f; // Actually negative max, but we'll use positive to make scaling easier
        [HideInInspector] private const float freqBoostMax = 40f; // Actually negative min, but we'll use positive to make scaling easier

        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        //private static extern bool WebGL_StartSampling(string uniqueName, float duration, int sampleSize);
        private static extern bool WebGL_StartSampling(string uniqueName, float duration, int sampleSize, string dataType);
    
        [DllImport("__Internal")]
        private static extern bool WebGL_StopSampling(string uniqueName);
    
        [DllImport("__Internal")]
        //private static extern bool WebGL_GetAmplitude(string uniqueName, float[] sample, int sampleSize);
        private static extern bool WebGL_GetAmplitude(string uniqueName, float[] sample, int sampleSize);

        [DllImport("__Internal")]
        //private static extern bool WebGL_GetFrequency(string uniqueName, float[] sample, int sampleSize);
        private static extern bool WebGL_GetFrequency(string uniqueName, float[] sample);
        #endif

        private void Awake()
        {         
            if (audioSource)
            {
                if (audioSource.playOnAwake)
                {
                    audioSource.Stop();
                    run = false;
                }
            }
        }

        private void Start()
        {
            sample = new float[sampleSize];

            if (audioSource)
                if (audioSource.playOnAwake)
                    StartCoroutine(PlayOnAwake(playOnAwakeDelay));
                else
                    run = true;
        }

        IEnumerator PlayOnAwake(float delay)
        {
            audioSource.Play();
            yield return new WaitForSeconds(delay);
            run = true;
        }

        private void LateUpdate()
        {            
            if (audioSource && run)
            {          
                // Reinitialize the sample array if the sampleSize is changed
                if (sample.Length != sampleSize)
                    sample = new float[sampleSize];
                
                // AudioSource started playing
                if (audioSource.clip != null)
                {
                    if (audioSource.clip.loadState == AudioDataLoadState.Loaded)
                    {
                        if (audioSource.isPlaying && audioSource.isPlaying != prevPlayState)
                        {
                            #if UNITY_WEBGL && !UNITY_EDITOR
                            WebGL_StartSampling(audioSource.GetInstanceID().ToString(), audioSource.clip.length, sampleSize, dataType.ToString());
                            #endif
        
                            prevPlayState = audioSource.isPlaying;
                        }
                    }
                }

                // AudioSource is playing
                if (audioSource.clip != null)
                {
                    if (audioSource.clip.loadState == AudioDataLoadState.Loaded)
                    {
                        if (audioSource.isPlaying)
                        {
                            #if UNITY_WEBGL && !UNITY_EDITOR
                            if (dataType == DataType.Amplitude)
                                // WebGL_GetAmplitude(audioSource.GetInstanceID().ToString(), sample, sampleSize);
                                WebGL_GetAmplitude(audioSource.GetInstanceID().ToString(), sample, sampleSize);
                            else // Frequency
                                // WebGL_GetFrequency(audioSource.GetInstanceID().ToString(), sample, sampleSize);
                                WebGL_GetFrequency(audioSource.GetInstanceID().ToString(), sample);
                            #else
                            if (dataType == DataType.Amplitude)
                                audioSource.GetOutputData(sample, 0);
                            else // Frequency
                                AudioListener.GetSpectrumData(sample, 0, FFTWindow.Rectangular);
                            #endif
        
                            if (sample != null)
                            {
                                avgTotal = 0;
                                maxTotal = 0;
                                for (int i=0; i<sampleSize; i++)
                                {
                                    // Frequency is absolute values only
                                    if (dataType == DataType.Frequency)
                                        absoluteValues = true;
        
                                    // Get the sample value
                                    tempVal = sample[i];
                                    // Get the original sign
                                    sign = Mathf.Sign(tempVal);
        
                                    if (absoluteValues) // Waveform or Frequency
                                    {
                                        if (dataType == DataType.Amplitude)
                                        {
                                            // Get the absolute value
                                            tempVal = Mathf.Abs(tempVal);
                                            // Add boost
                                            tempVal = Mathf.Pow(tempVal, boostBase - boost);
                                            // Write boosted value back to the original sample
                                            sample[i] = tempVal;
                                        }
                                        else // Frequency
                                        {
                                            #if UNITY_WEBGL && !UNITY_EDITOR
                                            freqBoost = ScaleRange(boost, 0f, 1f, freqBoostMin, freqBoostMax);
                                            freqBoost = freqBoost * -1;
                                            tempVal = freqBoost / tempVal;
                                            tempVal = Mathf.Clamp(tempVal, 0f, 1f);
                                            sample[i] = tempVal;
                                            #else
                                            // Add boost
                                            tempVal = Mathf.Pow(tempVal, boostBase - boost);
                                            // Write boosted value back to the original sample
                                            sample[i] = tempVal;
                                            #endif
                                        }
                                    }
                                    else // Waveform not absolute
                                    {
                                        // Add boost
                                        tempVal = Mathf.Pow(tempVal, boostBase - boost);
                                        // Write the boosted and sign corrected value back to the original sample
                                        sample[i] = tempVal * sign;
                                    }

                                    avgTotal += sample[i];
                                    
                                    if (sample[i] > maxTotal)
                                        maxTotal = sample[i];
                                }

                                average = avgTotal / sampleSize;                                
                                max = maxTotal;
                            }
                            else
                            {
                                Debug.Log("sample is null");
                            }
                        }
                    }
                }

                // AudioSource stopped playing
                if (!audioSource.isPlaying && audioSource.isPlaying != prevPlayState)
                {
                    #if UNITY_WEBGL && !UNITY_EDITOR
                    WebGL_StopSampling(audioSource.GetInstanceID().ToString());
                    #endif

                    // clear values
                    for (int i=0; i<sampleSize; i++)
                    {
                        sample[i] = 0;
                    }
                    average = 0;

                    prevPlayState = audioSource.isPlaying;
                }
            }
        }

        /// <summary>
        /// Scale range and return and absolute value
        /// </summary>
        /// <param name="val"></param>
        /// <param name="inMin"></param>
        /// <param name="inMax"></param>
        /// <param name="outMin"></param>
        /// <param name="outMax"></param>
        /// <returns></returns>
        public static float ScaleRange(float val, float inMin, float inMax, float outMin, float outMax)
        {
            val = Mathf.Clamp(val, inMin, inMax);
            float scaled = (((val - inMin) * (outMax - outMin)) / (inMax - inMin)) + outMin;
            return Mathf.Clamp(Mathf.Abs(scaled), outMin, outMax);
        }
    }
}