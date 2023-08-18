using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NtooManager))]
public class NtooManagerEditor : Editor
{
  private bool init = true;
  private int selectedGreeting = 0;
  private bool unfolded = true;
  NtooManager targetManager;

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    if (init)
    {
      init = false;
      targetManager = (NtooManager)target;
    }

    EditorGUI.BeginChangeCheck();

    unfolded = EditorGUILayout.Foldout(unfolded, "NTOO Debug Console");
    if (unfolded)
    {
      EditorGUILayout.Space();

      // Proximity Simulation
      GUILayout.Label("Proximity Simulation");
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Proximal"))
      {
        targetManager.UserPresent = true;
      }

      if (GUILayout.Button("Not Proximal"))
      {
        targetManager.UserPresent = false;
      }
      GUILayout.EndHorizontal();

      EditorGUILayout.Space();

      // Trigger Greeting
      GUILayout.Label("Trigger Greeting");
      string[] greetingMessages = new string[targetManager.Greetings.Length];
      for (int i = 0; i < greetingMessages.Length; i++)
      {
        greetingMessages[i] = targetManager.Greetings[i].message;
      }
      selectedGreeting = EditorGUILayout.Popup("Select Message", selectedGreeting, greetingMessages);
      if (GUILayout.Button("Send"))
      {
        targetManager.TriggerConversationLoop(selectedGreeting);
      }
    }
    EditorGUILayout.EndFoldoutHeaderGroup();
  }
}
