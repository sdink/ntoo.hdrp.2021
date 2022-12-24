using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LiveCharacterSetupFile))]
public class LiveCharacterSetupFileEditor : Editor
{
    GUIStyle warningStyle;
    LiveCharacterSetupFile setupFile;
    //BaseFacewareExpressions baseExpressions;
    bool needsSaving = false;

    private void OnEnable()
    {
        setupFile = (LiveCharacterSetupFile)target;

        warningStyle = new GUIStyle();
        warningStyle.fontStyle = FontStyle.Bold;
        warningStyle.normal.textColor = Color.red;
        warningStyle.wordWrap = true;
        warningStyle.alignment = TextAnchor.MiddleCenter;

        //string baseFacewareExpressionsPath = Utility.CombinePath("Assets", "Faceware", "Scripts", "Editor", "BaseFacewareExpressions.asset");
        //baseExpressions = (BaseFacewareExpressions)AssetDatabase.LoadAssetAtPath(baseFacewareExpressionsPath, typeof(BaseFacewareExpressions));
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("STOP! MODIFIYING THIS FILE CAN MAKE THE FACEWARE LIVE PLUGIN UNUSABLE. CHANGE AT YOUR OWN RISK!", warningStyle);

        EditorGUILayout.LabelField(setupFile.Application + " Character Setup File " + setupFile.Version);
        EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(target.GetInstanceID()));

        if(needsSaving)
        {
            if (GUILayout.Button("Save Changes"))
            {
                EditorUtility.SetDirty(setupFile);
                AssetDatabase.SaveAssets();
                needsSaving = false;
            }
        }
        SerializedProperty controlList = serializedObject.FindProperty("Controls");

        GUILayout.BeginHorizontal("Box");
        EditorGUILayout.LabelField("Total Controls: " + setupFile.Controls.Count);
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(controlList, true);

        //TODO - Render expressions in a custom way so that base expressions can't be removed
        GUILayout.BeginHorizontal("Box");
        GUILayout.Label("Total Expressions: " + setupFile.Expressions.Count);
        //if(GUILayout.Button("Add New Expression (Advanced Users Only)"))
        //{
        //    AddExpression();
        //}
        GUILayout.EndHorizontal();

        //TODO - REMOVE THIS GENERIC LIST RENDERING CODE AND REPLACE WITH CUSTOM RENDER

        SerializedProperty expressionList = serializedObject.FindProperty("Expressions");
        EditorGUILayout.PropertyField(expressionList, true);

        //END TODO

        //for(int i = 0; i < setupFile.Expressions.Count; i++)
        //{
        //    //If its a base expression don't allow editing
        //    bool isBaseExpression = baseExpressions.IsABaseExpression(setupFile.Expressions[i].Attr);
        //
        //
        //}

        setupFile.rootObject = (GameObject)EditorGUILayout.ObjectField("Root object: ", setupFile.rootObject, typeof(GameObject), true);
        EditorGUILayout.LabelField("Root object name: " + setupFile.rootObjectName);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            needsSaving = true;
        }

        EditorGUILayout.LabelField("STOP! MODIFIYING THIS FILE CAN MAKE THE FACEWARE LIVE PLUGIN UNUSABLE. CHANGE AT YOUR OWN RISK!", warningStyle);
    }

    void AddExpression()
    {
        setupFile.Expressions.Add(new Expression());
    }
}
