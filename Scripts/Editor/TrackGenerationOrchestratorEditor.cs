using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackGenerationOrchestrator), true)]
public class TrackGenerationOrchestratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (GUILayout.Button("Generate Track", GUILayout.Height(30)))
        {
            ((TrackGenerationOrchestrator)target).GenerateTrack();
        }

        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }
}