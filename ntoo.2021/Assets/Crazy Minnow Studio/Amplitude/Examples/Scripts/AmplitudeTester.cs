using UnityEngine;
using UnityEngine.UI; // Needed to display the amplitude data in a Unity UI Slider
using CrazyMinnow.AmplitudeWebGL; // Import the AmplitudeWebGL namespace

namespace CrazyMinnow.AmplitudeWebGL
{	
	public class AmplitudeTester : MonoBehaviour 
	{
		public Amplitude amplitude; // Reference to the Amplitude component
		public Slider uiSlider; // Reference to a Unity UI Slider component to display amplitude data

		// Read the amplitude sample or average values 
		// while the AudioSource AudioClip is playing
		void Update() 
		{
			// Only read Amplitude values when the AudioSource is playing
			if (amplitude.audioSource.isPlaying)
			{
				// Access the amplitude average
				uiSlider.value = amplitude.average;

				// Or access the sample array
				// for (int i=0; i<amplitude.sample.Length; i++)
				// {
				// 	uiSlider.value = sample[i];
				// }
			}
		}

		// Example method calls the AudioSource.Play method through the Amplitude AudioSource reference
		public void Play()
		{
			amplitude.audioSource.Play();
		}

        // Example method calls the AudioSource.Stop method through the Amplitude AudioSource reference
        public void Stop()
		{
			amplitude.audioSource.Stop();
		}
	}
}