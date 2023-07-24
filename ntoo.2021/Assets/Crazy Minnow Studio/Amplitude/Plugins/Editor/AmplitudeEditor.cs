using UnityEngine;
using UnityEditor;

namespace CrazyMinnow.AmplitudeWebGL
{
	[CustomEditor(typeof(Amplitude))]
	public class AmplitudeEditor : Editor
	{
		private Amplitude instance;
		private Texture inspLogo;

		public void OnEnable()
		{
			instance = target as Amplitude;
			inspLogo = (Texture2D)Resources.Load("Amplitude");
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Space(5);
			GUILayout.Box(new GUIContent(inspLogo), new GUIStyle(), new GUILayoutOption[] { GUILayout.Height(35) });

			EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Space(5);

                instance.audioSource = (AudioSource)EditorGUILayout.ObjectField(
                    new GUIContent("AudioSource", "Link to the AudioSource component you wish to analyze"),
                    instance.audioSource, typeof(AudioSource), true);

                instance.dataType = (Amplitude.DataType)EditorGUILayout.EnumPopup("Data Type", instance.dataType);

                instance.sampleSize = EditorGUILayout.IntPopup("Sample Size", instance.sampleSize, 
                    instance.sampleSizeNames, instance.sampleSizeVals);

                instance.boost = EditorGUILayout.Slider(
					new GUIContent("Boost", "Adjust the boost of the amplitude levels."), 
					instance.boost, 0f, 1f);

                if (instance.dataType == Amplitude.DataType.Amplitude)
                {
				    instance.absoluteValues = EditorGUILayout.Toggle(
					    new GUIContent("Absolute Values", "Force the output array to use absolute (positive) values only."),
					    instance.absoluteValues);
                }
                else
                {
                    instance.absoluteValues = false;
                }

				GUILayout.Space(5);
			}
			EditorGUILayout.EndVertical();
		}
	}
}