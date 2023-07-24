using UnityEngine;
using UnityEditor;

namespace CrazyMinnow.SALSA.TextSync
{
	[CustomEditor(typeof(SalsaTextSync))]
	public class SalsaTextSyncEditor : Editor
	{
		private SalsaTextSync salsaTextSync; // Instance

		public void OnEnable()
		{
			salsaTextSync = target as SalsaTextSync; // Get instance
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			{
				GUILayout.Space(5f);
				salsaTextSync.usePreferredSalsaSettings =
					GUILayout.Toggle(salsaTextSync.usePreferredSalsaSettings, new GUIContent((salsaTextSync.usePreferredSalsaSettings ? "Using" : "Use") + " preferred SALSA settings.", "Applies recommended SALSA settings at runtime to work best with SalsaTextSync."));

				salsaTextSync.salsa =
					(Salsa)EditorGUILayout.ObjectField(new GUIContent("SALSA LipSync", "SALSA reference"),
													   salsaTextSync.salsa,
													   typeof(Salsa), true);

				salsaTextSync.wordsPerMinute =
					EditorGUILayout.FloatField(new GUIContent("Words Per Minute", "Words per minute"),
											   salsaTextSync.wordsPerMinute);

				EditorGUILayout.LabelField(new GUIContent("Say", "public void Say(string text)"));

                var origWrap = EditorStyles.textField.wordWrap;
                EditorStyles.textField.wordWrap = true;
				salsaTextSync.text = EditorGUILayout.TextArea(salsaTextSync.text, GUILayout.MaxHeight(75));
                EditorStyles.textField.wordWrap = origWrap;

				if (Application.isPlaying)
				{
					EditorGUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("Say"))
							salsaTextSync.Say(salsaTextSync.text);

						if (GUILayout.Button("Stop"))
							salsaTextSync.Stop();
					}
					EditorGUILayout.EndHorizontal();
				}
				GUILayout.Space(10);
			}
			EditorGUILayout.EndVertical();
		}
	}
}