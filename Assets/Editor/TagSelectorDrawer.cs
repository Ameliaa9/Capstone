using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

        int currentIndex = 0;
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i] == property.stringValue)
            {
                currentIndex = i;
                break;
            }
        }

        currentIndex = EditorGUI.Popup(position, label.text, currentIndex, tags);
        property.stringValue = tags[currentIndex];

        EditorGUI.EndProperty();
    }
}