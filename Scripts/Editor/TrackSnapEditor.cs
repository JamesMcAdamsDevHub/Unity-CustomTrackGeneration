#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TrackSnapEditor
{
    static TrackSnapEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Only trigger on mouse release
        if (e.type != EventType.MouseUp)
            return;

        // Get selected object
        Transform selected = Selection.activeTransform;
        if (selected == null)
            return;

        TrackGenerationOrchestrator track =
            selected.GetComponent<TrackGenerationOrchestrator>();

        if (track == null)
            return;

        track.DisconnectTracks();

        track.TrySnap(); 
    }
}
#endif
