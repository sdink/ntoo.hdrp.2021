using UnityEngine;
using UnityEditor;
using CrazyMinnow.AmplitudeWebGL;

namespace CrazyMinnow.SALSA.AmplitudeWebGL
{
	[CustomEditor(typeof(AmplitudeSALSA))]
	public class AmplitudeSALSAEditor : Editor
	{
		private AmplitudeSALSA instance;
		private Texture inspLogo;

		public void OnEnable()
		{
			instance = target as AmplitudeSALSA;
			if (instance) instance.SetupReferences();
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Space(5);
			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				GUILayout.Space(5);

				instance.salsa = (Salsa)EditorGUILayout.ObjectField(
					new GUIContent("Salsa", "Salsa LipSync reference."), instance.salsa, typeof(Salsa), true);

				instance.amplitude = (Amplitude)EditorGUILayout.ObjectField(
					new GUIContent("Amplitude", "Link to the Amplitude component for this character."),
					instance.amplitude, typeof(Amplitude), true);

				GUILayout.Space(5);
			}
			EditorGUILayout.EndVertical();
		}
	}
}