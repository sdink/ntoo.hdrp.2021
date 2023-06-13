using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UserPresence))]
public class UserPresenceEditor : Editor
{
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    EditorGUI.BeginChangeCheck();

    EditorGUILayout.Space();

    // Proximity Simulation
    GUILayout.Label("Proximity Simulation");
    GUILayout.BeginHorizontal();
    if (GUILayout.Button("Proximal"))
    {
      UserPresence targetPresence = target as UserPresence;
      targetPresence.UserPresent = true;
    }

    if (GUILayout.Button("Not Proximal"))
    {
      UserPresence targetPresence = target as UserPresence;
      targetPresence.UserPresent = false;
    }
    GUILayout.EndHorizontal();

    EditorGUILayout.Space();
  }
}
