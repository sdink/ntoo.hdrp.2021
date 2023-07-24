using UnityEngine;

namespace CrazyMinnow.SALSA.OneClicks
{
	public class OneClickDAZ : OneClickBase
	{
		/// <summary>
		/// Setup and run OneClick operation on the supplied GameObject.
		/// </summary>
		/// <param name="gameObject">Root OneClick GameObject.</param>
		/// <param name="clip">AudioClip (can be null).</param>
		public static void Setup(GameObject gameObject, AudioClip clip)
		{
			////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//	SETUP Requirements:
			//		use NewViseme("viseme name") to start a new viseme.
			//		use AddShapeComponent to add blendshape configurations, passing:
			//			- string array of shape names to look for.
			//			- optional string name prefix for the component.
			//			- optional blend amount (default = 1.0f).

			Init();

			#region SALSA-Configuration

			NewConfiguration(OneClickConfiguration.ConfigType.Salsa);
			{
				////////////////////////////////////////////////////////
				// SMR regex searches (enable/disable/add as required).
				AddSmrSearch("^genesis([238])?(fe)?(male)?\\.shape$");
				AddSmrSearch("^genesis[238](fe)?maleeyelashes\\.shape$");
				AddSmrSearch("^emotiguy.*\\.shape$");

				////////////////////////////////////////////////////////
				// Adjust SALSA settings to taste...
				// - data analysis settings
				autoAdjustAnalysis = true;
				autoAdjustMicrophone = false;
				// - advanced dynamics settings
				loCutoff = 0.045f;
				hiCutoff = 0.75f;
				useAdvDyn = true;
				advDynPrimaryBias = 0.45f;
				useAdvDynJitter = true;
				advDynJitterAmount = 0.1f;
				advDynJitterProb = 0.25f;
				advDynSecondaryMix = 0.271f;
				emphasizerTrigger = 0.2f;


				////////////////////////////////////////////////////////
				// Viseme setup...


				NewExpression("w");
				AddShapeComponent(new[] {"head__eCTRLvW", "head__VSMW", "head__CTRLVSMW"}, 0.08f, 0f, 0.06f, "head__eCTRLvW", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__PuckerLipsWide"}, 0.08f, 0f, 0.06f, "head__PuckerLipsWide", 1f);


				NewExpression("t");
				AddShapeComponent(new[] {"head__eCTRLvT", "head__VSMT", "head__CTRLVSMT"}, 0.08f, 0f, 0.06f, "head__eCTRLvT", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__MouthTH"}, 0.08f, 0f, 0.06f, "head__MouthTH", 0.709f);
				AddShapeComponent(new[] {"head__PuckerLips"}, 0.08f, 0f, 0.06f, "head__PuckerLips", 0.531f);


				NewExpression("f");
				AddShapeComponent(new[] {"head__eCTRLvF", "head__VSMF", "head__CTRLVSMF"}, 0.08f, 0f, 0.06f, "head__eCTRLvF", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__MouthF"}, 0.08f, 0f, 0.06f, "head__MouthF", 1f);
				AddShapeComponent(new[] {"head__MouthTH"}, 0.08f, 0f, 0.06f, "head__MouthTH", 0.231f);


				NewExpression("th");
				AddShapeComponent(new[] {"head__eCTRLvTH", "head__VSMTH", "head__CTRLVSMTH"}, 0.08f, 0f, 0.06f, "head__eCTRLvTH", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__PuckerLips"}, 0.08f, 0f, 0.06f, "head__PuckerLips", 0.419f);
				AddShapeComponent(new[] {"head__MouthTH"}, 0.08f, 0f, 0.06f, "head__MouthTH", 1f);


				NewExpression("ow");
				AddShapeComponent(new[] {"head__eCTRLvOW", "head__VSMOW", "head__CTRLVSMOW"}, 0.08f, 0f, 0.06f, "head__eCTRLvOW", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__MouthO"}, 0.08f, 0f, 0.06f, "head__MouthO", 1f);
				AddShapeComponent(new[] {"head__StretchLips"}, 0.08f, 0f, 0.06f, "head__StretchLips", 0.538f);


				NewExpression("ee");
				AddShapeComponent(new[] {"head__eCTRLvEE", "head__VSMEH", "head__CTRLVSMEH"}, 0.08f, 0f, 0.06f, "head__eCTRLvEE", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__PuckerLipsWide"}, 0.08f, 0f, 0.06f, "head__PuckerLipsWide", 0.762f);
				AddShapeComponent(new[] {"head__StretchLips"}, 0.08f, 0f, 0.06f, "head__StretchLips", 1f);
				AddShapeComponent(new[] {"head__MouthSpeak"}, 0.08f, 0f, 0.06f, "head__MouthSpeak", 0.36f);


				NewExpression("oo");
				AddShapeComponent(new[] {"head__eCTRLvOW", "head__VSMOW", "head__CTRLVSMOW"}, 0.08f, 0f, 0.06f, "head__eCTRLvOW", 1f);
				AddShapeComponent(new[] {"head__eCTRLMouthWide-Narrow"}, 0.08f, 0f, 0.06f, "head__eCTRLMouthWide-Narrow", 1f);
				AddShapeComponent(new[] {"head__eCTRLvAA", "head__VSMAA", "head__CTRLVSMAA"}, 0.08f, 0f, 0.06f, "head__eCTRLvAA", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__PuckerLipsOOO"}, 0.08f, 0f, 0.06f, "head__PuckerLipsOOO", 0.537f);
				AddShapeComponent(new[] {"head__MouthYell"}, 0.08f, 0f, 0.06f, "head__MouthYell", 0.795f);

				#endregion // SALSA-configuration
			}

			#region EmoteR-Configuration

			NewConfiguration(OneClickConfiguration.ConfigType.Emoter);
			{
				////////////////////////////////////////////////////////
				// SMR regex searches (enable/disable/add as required).
				AddSmrSearch("^genesis([238])?(fe)?(male)?\\.shape$");
				AddSmrSearch("^genesis[238](fe)?maleeyelashes\\.shape$");
				AddSmrSearch("^emotiguy.*\\.shape$");

				useRandomEmotes = true;
				isChancePerEmote = true;
				numRandomEmotesPerCycle = 0;
				randomEmoteMinTimer = 1f;
				randomEmoteMaxTimer = 2f;
				randomChance = 0.5f;
				useRandomFrac = false;
				randomFracBias = 0.5f;
				useRandomHoldDuration = false;
				randomHoldDurationMin = 0.1f;
				randomHoldDurationMax = 0.5f;


				NewExpression("exasper");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"head__CTRLCheeksBalloon", "head__eCTRLCheeksBalloonPucker"}, 0.25f, 0.1f, 0.2f, "cheeks", 0.746f);
				AddShapeComponent(new[] {"head__PHMCheeksBalloon"}, 0.25f, 0.1f, 0.2f, "cheeks", 0.746f);
				AddShapeComponent(new[] {"head__eCTRLCheekFlex-Slack"}, 0.25f, 0.1f, 0.2f, "cheeks", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__EyesWide"}, 0.25f, 0.1f, 0.2f, "head__EyesWide", 0.6f);


				NewExpression("soften");
				AddEmoteFlags(false, true, false, 0.751f);
				AddShapeComponent(new[] {"head__eCTRLSmileFullFace_HD", "head__eCTRLSmile", "head__PHMSmileFullFace"}, 0.25f, 0.1f, 0.2f, "smile", 0.756f);
				AddShapeComponent(new[] {"head__CTRLMouthSmile"}, 0.25f, 0.1f, 0.2f, "smile", 1.0f);
				AddShapeComponent(new[] {"head__CTRLEyeLidsBottomUp"}, 0.25f, 0.1f, 0.2f, "smile", 80f);
				// emotiguy
				AddEmoteFlags(false, true, false, 0.694f);
				AddShapeComponent(new[] {"head__Smile"}, 0.25f, 0.1f, 0.2f, "head__Smile", 0.6f);


				NewExpression("browsUp");
				AddEmoteFlags(false, true, false, 0.886f);
				AddShapeComponent(new[] {"head__eCTRLShock_HD"}, 0.25f, 0.1f, 0.2f, "browsUp", 0.689f);
				AddShapeComponent(new[] {"head__eCTRLBrowUp-DownR", "head__CTRLBrowUpR", "head__PHMBrowUpR"}, 0.2f, 0.1f, 0.2f, "smile", .80f);
				AddShapeComponent(new[] {"head__eCTRLBrowUp-DownL", "head__CTRLBrowUpL", "head__PHMBrowUpL"}, 0.25f, 0.05f, 0.2f, "smile", .65f);
				// emotiguy
				AddShapeComponent(new[] {"head__DontTell"}, 0.25f, 0.1f, 0.2f, "head__DontTell", 0.43f);


				NewExpression("browUp");
				AddEmoteFlags(false, true, false, .808f);
				AddShapeComponent(new[] {"head__eCTRLBrowUp-DownR", "head__PHMBrowOuterUpR", "head__CTRLBrowOuterUpR"}, 0.2f, 0.1f, 0.15f, "head__eCTRLBrowUp-DownR", 0.9f);
				AddShapeComponent(new[] {"head_PHMBrowInnerUpR", "head__CTRLBrowInnerUpR"}, 0.2f, 0.1f, 0.15f, "head__eCTRLBrowUp-DownR", 0.9f);
				AddShapeComponent(new[] {"head__eCTRLBrowUp-DownL", "head__PHMBrowInnerUpL", "head__CTRLBrowInnerUpL"}, 0.2f, 0.1f, 0.15f, "head__eCTRLBrowUp-DownL", 0.164f);
				AddShapeComponent(new[] {"head__eCTRLEyelidsUpperUp-DownR", "head__PHMEyeLidsTopUpR", "head__CTRLEyeLidsTopUpR"}, 0.2f, 0.1f, 0.15f, "head__eCTRLEyelidsUpperUp-DownR", 1f);
				AddShapeComponent(new[] {"head__eCTRLCheekEyeFlexL", "head__eCTRLCheekFlexL", "head__PHMCheekFlexL", "head__CTRLCheekFlexL"}, 0.2f, 0.1f, 0.15f, "head__eCTRLCheekEyeFlexL", 1f);
				AddShapeComponent(new[] {"head__eCTRLMouthSmileSimpleR", "head__CTRLSimpleSmileR"}, 0.2f, 0.1f, 0.15f, "head__eCTRLMouthSmileSimpleR", 1f);
				// emotiguy
				AddShapeComponent(new[] {"head__Brows-Tilt_r"}, 0.25f, 0.1f, 0.2f, "head__Brows-Tilt_r", 0.569f);
				AddShapeComponent(new[] {"head__Brows-Tilt_l"}, 0.25f, 0.1f, 0.2f, "head__Brows-Tilt_l", 0.652f);
				AddShapeComponent(new[] {"head__Brows-UpDown_r"}, 0.25f, 0.1f, 0.2f, "head__Brows-UpDown_r", 0.777f);
				AddShapeComponent(new[] {"head__EyesWide"}, 0.25f, 0.1f, 0.2f, "head__EyesWide", 0.229f);


				NewExpression("squint");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"head__eCTRLEyesSquint-Widen", "head__eCTRLEyesSquint", "head__PHMEyesSquintR", "head__CTRLEyesSquint"}, 0.2f, 0.1f, 0.15f,"head__eCTRLEyesSquint-Widen", 0.6f);
				AddShapeComponent(new[] {"head__PHMEyesSquintL"}, 0.2f, 0.1f, 0.15f,"head__eCTRLEyesSquint-Widen", 0.6f);
				// emotiguy
				AddShapeComponent(new[] {"head__Wink L"}, 0.25f, 0.1f, 0.2f, "head__Wink L", 0.174f);
				AddShapeComponent(new[] {"head__Wink R"}, 0.25f, 0.1f, 0.2f, "head__Wink R", 0.247f);


				NewExpression("focus");
				AddEmoteFlags(false, true, false, 766f);
				AddShapeComponent(new[] {"head__eCTRLCheekEyeFlex", "head__PHMCheekEyeFlexR", "head__CTRLCheeksEyeFlex"}, 0.2f, 0.1f, 0.15f, "head__eCTRLCheekEyeFlex",0.71f);
				AddShapeComponent(new[] {"head__PHMCheekEyeFlexL"}, 0.2f, 0.1f, 0.15f, "head__eCTRLCheekEyeFlex",0.71f);
				// emotiguy
				AddShapeComponent(new[] {"head__Nerd"}, 0.25f, 0.1f, 0.2f, "head__Nerd", 0.6f);


				NewExpression("scrunch");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"head__PHMNoseCompression_HD_div2"}, 0.2f, 0.1f, 0.15f,"wrinkle", 1f);
				AddShapeComponent(new[] {"head__eCTRLNoseWrinkle", "head__PHMNoseWrinkle", "head__CTRLNoseWrinkle"}, 0.2f, 0.1f, 0.15f,"wrinkle", .7f);
				// emotiguy none


				NewExpression("flare");
				AddEmoteFlags(false, true, false, 1f);
				AddShapeComponent(new[] {"head__eCTRLNostrilsFlex", "head__eCTRLNostrilsFlare", "head__PHMNostrilsFlare", "head__CTRLNostrilsFlare"}, 0.2f, 0.1f, 0.15f, "nostrilflex", 1f);
				AddShapeComponent(new[] {"head__eCTRLCheekFlex-Slack", "head__eCTRLCheekFlex", "head__PHMCheekFlexR", "head__CTRLCheeksFlex"}, 0.2f, 0.1f, 0.15f, "cheekflex", 1f);
				AddShapeComponent(new[] {"head__PHMCheekFlexL"}, 0.2f, 0.1f, 0.15f, "checkflexL",1f);
				// emotiguy none

				#endregion // EmoteR-configuration

				DoOneClickiness(gameObject, clip);
			}
		}
	}
}