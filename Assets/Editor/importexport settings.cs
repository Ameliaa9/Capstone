using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlexibleBikeLegs_MultiDeform))]
public class FlexibleBikeLegs_MultiDeformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Settings Management", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        FlexibleBikeLegs_MultiDeform script = (FlexibleBikeLegs_MultiDeform)target;

        Color originalColor = GUI.backgroundColor;

        // Export button
        GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
        if (GUILayout.Button("Export Settings to JSON", GUILayout.Height(30)))
        {
            script.ExportSettings();
        }

        EditorGUILayout.Space(5);

        // Import button
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Import Settings from JSON", GUILayout.Height(30)))
        {
            script.ImportSettings();
        }

        EditorGUILayout.Space(5);

        // Reset button
        GUI.backgroundColor = new Color(0.9f, 0.5f, 0.3f);
        if (GUILayout.Button("Reset Runtime Data", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Reset",
                "This will clear all runtime mesh data.",
                "Reset", "Cancel"))
            {
                script.ResetLegs();
            }
        }

        // Reset background color
        GUI.backgroundColor = originalColor;
    }
}