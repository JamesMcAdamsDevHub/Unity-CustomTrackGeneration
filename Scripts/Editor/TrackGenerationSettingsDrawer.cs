using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TrackGenerationSettings))]
public class TrackGenerationSettingsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUILayout.BeginVertical();

        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
        if (!property.isExpanded)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        var useConfigProp = property.FindPropertyRelative("useConfig");
        var trackConfigProp = property.FindPropertyRelative("trackConfig");

        EditorGUILayout.PropertyField(useConfigProp);

        if (useConfigProp.boolValue)
        {
            EditorGUILayout.PropertyField(trackConfigProp);
        }

        EditorGUI.BeginDisabledGroup(useConfigProp.boolValue);

        EditorGUILayout.PropertyField(property.FindPropertyRelative("trackWidth"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("trackHeight"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("railWidth"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("railRidgeHeight"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("railRidgePosition"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("distanceBetweenRings"));

        EditorGUILayout.PropertyField(property.FindPropertyRelative("deckMaterial"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("deckMaterialTileSize"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("railMaterial"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("railMaterialTileSize"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("baseMaterial"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("baseMaterialTileSize"));

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndVertical();
    }
}