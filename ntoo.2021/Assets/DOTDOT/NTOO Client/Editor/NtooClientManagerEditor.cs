using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NtooClientManager))]
public class ClientManagerEditor : Editor
{
    private AudioClip injectSendAudioClip;
    private AudioClip injectReceiveAudioClip;
    GUIContent injectSendAudioClipLabel = new GUIContent("Audio Clip: ", "The audio clip to send");
    GUIContent injectReceiveAudioClipLabel = new GUIContent("Audio Clip: ", "The audio clip to trigger the response with");

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NtooClientManager targetManager = (NtooClientManager)target;

        // Simulate sending an Audio File
        GUILayout.Label("Simulate Sending");
        injectSendAudioClip = (AudioClip)EditorGUILayout.ObjectField(injectSendAudioClipLabel, injectSendAudioClip, typeof(AudioClip), false);
        
        if (GUILayout.Button("Simulate Send Event"))
        {
            targetManager.StreamAudio(injectSendAudioClip);
        }

        // Simulate Receiving an Audio File
        GUILayout.Label("Simulate Receiving");
        injectReceiveAudioClip = (AudioClip)EditorGUILayout.ObjectField(injectReceiveAudioClipLabel, injectReceiveAudioClip, typeof(AudioClip), false);
        
        if (GUILayout.Button("Simulate Receive Event"))
        {
            targetManager.PlayAudioClip.Invoke(injectReceiveAudioClip);
        }
    }
}
