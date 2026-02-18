using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MultiLegRigger))]
public class MultiLegRiggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var rigger = (MultiLegRigger)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(20);

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("AUTO-FIND LEGS", GUILayout.Height(35)))
        {
            Undo.RecordObject(rigger, "Auto Find");
            rigger.AutoFindLegs();
            EditorUtility.SetDirty(rigger);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("RIG ALL CHARACTERS", GUILayout.Height(50)))
        {
            Undo.RecordObject(rigger, "Rig All");
            rigger.RigAll();
            EditorUtility.SetDirty(rigger);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("CLEAR ALL BONES", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Clear All Bones?",
                "This will delete all created bones and restore original meshes. Continue?",
                "Yes", "No"))
            {
                Undo.RecordObject(rigger, "Clear Bones");
                rigger.ClearAllBones();
                EditorUtility.SetDirty(rigger);
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Clear Progress Only", GUILayout.Height(30)))
        {
            rigger.ClearAll();
            EditorUtility.SetDirty(rigger);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("STATUS", EditorStyles.boldLabel);

        foreach (var c in rigger.characters)
        {
            string left = c.leftDone ? "✓ DONE" : "○ pending";
            string right = c.rightDone ? "✓ DONE" : "○ pending";
            EditorGUILayout.LabelField($"{c.characterName}: Left {left} | Right {right}", EditorStyles.helpBox);
        }
    }
}