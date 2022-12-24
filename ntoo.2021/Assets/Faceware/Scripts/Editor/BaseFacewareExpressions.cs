using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class BaseFacewareExpressions : ScriptableObject {

    public List<Expression> BaseExpressions;

    public bool IsABaseExpression(string expression)
    {
        return false;
    }
}

[CustomEditor(typeof(BaseFacewareExpressions))]
public class BaseFacewareExpressionsEditor : Editor
{
    GUIStyle warningStyle;

    public void OnEnable()
    {
        warningStyle = new GUIStyle(EditorStyles.boldLabel);
        warningStyle.normal.textColor = Color.red;
        warningStyle.wordWrap = true;
        warningStyle.alignment = TextAnchor.MiddleCenter;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("STOP! MODIFIYING THIS FILE CAN MAKE THE FACEWARE LIVE PLUGIN UNUSABLE. CHANGE AT YOUR OWN RISK!", warningStyle);

        base.OnInspectorGUI();

        EditorGUILayout.LabelField("STOP! MODIFIYING THIS FILE CAN MAKE THE FACEWARE LIVE PLUGIN UNUSABLE. CHANGE AT YOUR OWN RISK!", warningStyle);
    }
}