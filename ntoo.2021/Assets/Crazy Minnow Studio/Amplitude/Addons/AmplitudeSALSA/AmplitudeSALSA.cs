using UnityEngine;
using CrazyMinnow.AmplitudeWebGL;

namespace CrazyMinnow.SALSA.AmplitudeWebGL
{
	[AddComponentMenu("Crazy Minnow Studio/Amplitude/Add-ons/AmplitudeSALSA")]
	public class AmplitudeSALSA : MonoBehaviour
	{
		public Salsa salsa;
		public Amplitude amplitude;

		private float timePulseCheck;
		private bool prevPlayState;

		/// <summary>
		/// Find the Amplitude/Salsa components on reset
		/// </summary>
		void Reset()
		{
			SetupReferences();
		}

		/// <summary>
		/// Ensure references are configured.
		/// </summary>
		void Awake()
		{
			SetupReferences();
		}

		/// <summary>
		/// Using the SALSA audioUpdateDelay, fetch the amplitude.average and write it to the SALSA analysis value.
		/// </summary>
		private void Update()
		{
			if (Time.time - timePulseCheck < salsa.audioUpdateDelay)
				return;

			timePulseCheck = Time.time;

			if (salsa && amplitude.audioSource.isPlaying)
				salsa.analysisValue = amplitude.average;
			else
				salsa.analysisValue = 0f;
		}

		/// <summary>
		/// Find the local Amplitude component
		/// </summary>
		public void SetupReferences()
		{
			if (!amplitude) amplitude = GetComponent<Amplitude>();
			if (!salsa) salsa = GetComponent<Salsa>();
			salsa.useExternalAnalysis = true;
		}
	}
}