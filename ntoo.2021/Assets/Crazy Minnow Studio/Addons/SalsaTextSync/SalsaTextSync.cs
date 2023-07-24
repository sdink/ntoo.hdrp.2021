using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CrazyMinnow.SALSA.TextSync
{
	[AddComponentMenu("Crazy Minnow Studio/SALSA LipSync/Add-ons/SalsaTextSync")]
	public class SalsaTextSync : MonoBehaviour
	{
		// ==========================================================================
		// PURPOSE: This script provides simple, simulated lip-sync input to the
		//		Salsa component from text/string values. For the latest information
		//		visit https://crazyminnowstudio.com and search for TextSync.
		// ==========================================================================
		// DISCLAIMER: While every attempt has been made to ensure the safe content
		//		and operation of these files, they are provided as-is, without
		//		warranty or guarantee of any kind. By downloading and using these
		//		files you are accepting any and all risks associated and release
		//		Crazy Minnow Studio, LLC of any and all liability.
		// ==========================================================================

		public Salsa salsa;
		public float wordsPerMinute = 130f; // Words per minute
		public string text; // The text used to perform text-to-lipsync
		public bool textSyncIsTalking = false; // Keeps track of talking status
		public bool usePreferredSalsaSettings = true;

		private float updateTimeCheck = 0.0f;
        private IEnumerator coroutine;

		private AudioSource salsaAudSrc;
		private List<float> salsaTriggers = new List<float>();
		private bool salsaAdvDyn = false;
		private float salsaAdvDynBias = 0.0f;
		private bool salsaUseAdvDynJitter = false;
		private float salsaLoCutoff = 0.0f;
		private float salsaHiCutoff = 1.0f;
		private bool salsaUseExternalAnalysis = false;
		private float salsaUpdateDelay = 0.08f;
		private bool settingsHaveBeenCollectedAtLeastOnce = false;

        /// <summary>
        /// Get triggers setup and start the UpdateShape coroutine
        /// </summary>
        void Start()
		{
			if (!salsa) salsa = GetComponent<Salsa>();
			if (salsa == null)
				Debug.LogWarning("SalsaTextSync requires a link to SALSA for operation.");
		}

		public void SetPreferredSalsaSettings()
		{
			CollectSalsaSettings();

			// advanced users: adjust these settings to preference
			salsa.DistributeTriggers(LerpEasings.EasingType.Linear);
			salsa.useExternalAnalysis = true;
			salsa.useAdvDyn = true;
			salsa.advDynPrimaryBias = 0.3f;
			salsa.useAdvDynJitter = false;
			salsa.loCutoff = 0.0f;
			salsa.hiCutoff = 1.0f;
			salsa.audioUpdateDelay = 0.08f;
		}

		private void CollectSalsaSettings()
		{
			if (salsa == null)
			{
				Debug.LogWarning("Salsa is not linked, settings cannot be gathered!");
				return;
			}

			salsaTriggers.Clear();
			foreach (var viseme in salsa.visemes)
				salsaTriggers.Add(viseme.trigger);

			salsaAudSrc = salsa.audioSrc;
			salsaUseExternalAnalysis = salsa.useExternalAnalysis;
			salsaAdvDyn = salsa.useAdvDyn;
			salsaAdvDynBias = salsa.advDynPrimaryBias;
			salsaUseAdvDynJitter = salsa.useAdvDynJitter;
			salsaLoCutoff = salsa.loCutoff;
			salsaHiCutoff = salsa.hiCutoff;
			salsaUpdateDelay = salsa.audioUpdateDelay;
			settingsHaveBeenCollectedAtLeastOnce = true;
		}

		public void ResetSalsaSettings()
		{
			if (!settingsHaveBeenCollectedAtLeastOnce)
			{
				if (salsa == null)
					Debug.LogWarning("Salsa is not linked, settings cannot be restored!");

				return;
			}

			if (salsa.visemes.Count != salsaTriggers.Count)
			{
				Debug.LogWarning("Cannot reset SALSA values, trigger count is not correct.");
				return;
			}

			for (int i = 0; i < salsa.visemes.Count; i++)
				salsa.visemes[i].trigger = salsaTriggers[i];

			salsa.audioSrc = salsaAudSrc;
			salsa.useExternalAnalysis = salsaUseExternalAnalysis;
			salsa.useAdvDyn = salsaAdvDyn;
			salsa.advDynPrimaryBias = salsaAdvDynBias;
			salsa.useAdvDynJitter = salsaUseAdvDynJitter;
			salsa.loCutoff = salsaLoCutoff;
			salsa.hiCutoff = salsaHiCutoff;
			salsa.audioUpdateDelay = salsaUpdateDelay;
		}

		/// <summary>
		/// Update average when audio-based lipsync is not active
		/// </summary>
		void Update()
		{
			// bail if no SALSA...
			if (!salsa)
				return;

			// bail if it's not time to update the analysis value...
			if (Time.time - updateTimeCheck < salsa.audioUpdateDelay)
				return;
			updateTimeCheck = Time.time;

			// if lips should be moving...fire a trigger value...
			// random range across [0.001f .. 1.0f] works well with Advanced Dynamics.
			if (textSyncIsTalking)
				salsa.analysisValue = Random.Range(0.001f, 1.0f);
			else
				salsa.analysisValue = 0.0f;

		}

		/// <summary>
		/// Sets talking status for duration of text sync
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		IEnumerator TalkTime(float duration)
		{
			textSyncIsTalking = true;
			yield return new WaitForSeconds(duration);
			textSyncIsTalking = false;

			if (usePreferredSalsaSettings)
				ResetSalsaSettings();
		}

		/// <summary>
		/// Call or send a message to this method, and pass a text string to perform text lipsync
		/// </summary>
		/// <param name="text"></param>
		public void Say(string text)
		{
			if (usePreferredSalsaSettings)
				SetPreferredSalsaSettings();

			this.text = text;

			if (this.text.Length > 0)
			{

				float timePerWord = (60f / wordsPerMinute);
				int wordCount = this.text.Split(' ').Length;

                coroutine = TalkTime(timePerWord * wordCount);
                StartCoroutine(coroutine);

                Debug.Log("Speaking the text. " + timePerWord + " " + wordCount);
			}

		}

        /// <summary>
        /// Stop talking
        /// </summary>
        public void Stop()
        {
			if (usePreferredSalsaSettings)
				ResetSalsaSettings();

			if (coroutine != null)
		        StopCoroutine(coroutine);
	        textSyncIsTalking = false;
        }
	}
}
