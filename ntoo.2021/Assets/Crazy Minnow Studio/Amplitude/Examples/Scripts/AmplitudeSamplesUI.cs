using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace CrazyMinnow.AmplitudeWebGL
{	
	public class AmplitudeSamplesUI : MonoBehaviour 
	{
		public Amplitude amplitude;
		public Slider eqAvgSlider;
		public Slider eqMaxSlider;
		public Slider[] eqSliders;

        private Text title;
        private Dropdown[] dropdowns;
        private Dropdown sampleSize;
        private Dropdown dataType;

        private void Start()
        {
            title = transform.GetComponentInChildren<Text>();
            dropdowns = transform.GetComponentsInChildren<Dropdown>();
            if (dropdowns != null)
            {
                for (int i = 0; i < dropdowns.Length; i++)
                {
                    if (dropdowns[i].gameObject.name == "sampleSize")
                        sampleSize = dropdowns[i];
                    if (dropdowns[i].gameObject.name == "dataType")
                        dataType = dropdowns[i];
                }
            }

            if (amplitude)
            {
                if (sampleSize)

                    sampleSize.value = System.Array.IndexOf(amplitude.sampleSizeVals, amplitude.sampleSize);

                if (dataType)
                    dataType.value = (int)amplitude.dataType;
            }
        }


        void Update () 
		{
            if (amplitude)
            {
                if (title != null)
                {
                    title.text = amplitude.dataType.ToString();
                }

			    if (eqAvgSlider != null)
                    eqAvgSlider.value = amplitude.average;
                else
                    Debug.LogError("Eq Avg is null");

                if (eqMaxSlider != null)
                    eqMaxSlider.value = amplitude.max;
                else
                    Debug.LogError("Eq Max is null");


                for (int i=0; i<amplitude.sample.Length; i++)
			    {
                    if (eqSliders != null)
                    {
                        if (i < eqSliders.Length)
					        eqSliders[i].value = amplitude.sample[i];
                    }
                    else
                    {
                        Debug.LogError("Eq Sliders is null");
                    }
			    }
            }
		}

        public void SetBoost(float boost)
        {
            amplitude.boost = boost;
        }

        public void OnValueChangedSampleSize(int sampleSizeIndex)
        {
            if (amplitude)
            {
                amplitude.sampleSize = amplitude.sampleSizeVals[sampleSizeIndex];
            }
        }

        public void OnValueChangedDataType(int dataType)
        {
            if (amplitude)
            {
                amplitude.dataType = (Amplitude.DataType)dataType;
            }
        }

        public void Play()
        {
            if (amplitude)
            {
                amplitude.audioSource.Play();
            }
        }

        public void Stop()
        {
            if (amplitude)
            {
                amplitude.audioSource.Stop();
            }
        }
	}
}