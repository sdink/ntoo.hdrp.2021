using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NtooManager))]
public class NtooManagerEditor : Editor
{
  private bool init = true;
  private int selectedMic = -1;
  private int prevSelectedMic = -1;
  private int selectedGreeting = 0;
  private bool unfolded = true;
  NtooManager targetManager;

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    if (init)
    {
      init = false;
      selectedMic = PlayerPrefs.GetInt("micDeviceIndex");
      // Prevent the Editor from updating the Mic from Prefs on Start, as the MicManager does this for itself:
      prevSelectedMic = selectedMic; // (see MicManager InitialiseMicrophone)
      targetManager = (NtooManager)target;
    }

    EditorGUI.BeginChangeCheck();

    unfolded = EditorGUILayout.Foldout(unfolded, "NTOO Debug Console");
    if (unfolded)
    {
      EditorGUILayout.Space();

      // Mic Selector
      selectedMic = EditorGUILayout.Popup("Select Mic", selectedMic, Microphone.devices);
      if (selectedMic != prevSelectedMic)
      {
        targetManager.UpdateMicrophoneDevice(selectedMic);
        prevSelectedMic = selectedMic;
        PlayerPrefs.SetInt("micDeviceIndex", selectedMic);
      }

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
      selectedGreeting = EditorGUILayout.Popup("Select Message", selectedGreeting, targetManager.Greetings);
      if (GUILayout.Button("Send"))
      {
        targetManager.TriggerConversationLoop(selectedGreeting);
      }
    }
    EditorGUILayout.EndFoldoutHeaderGroup();
  }
}
