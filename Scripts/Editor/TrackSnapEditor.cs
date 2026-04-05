#if UNITY_EDITOR
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

[InitializeOnLoad]
public static class TrackSnapEditor
{
    private static bool _isDragging;
    private static bool _mouseHeld;
    private static TrackGenerationOrchestrator _track;

    static TrackSnapEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e == null) return;

        if ((e.type == EventType.MouseDown && e.button == 0) || e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
        {
            _mouseHeld = true;
        }

        if ((e.type == EventType.MouseUp && e.button == 0) || e.type == EventType.DragExited)
        {
            _mouseHeld = false;
        }

        Transform selected = Selection.activeTransform;
        if (selected != null) _track = selected.GetComponent<TrackGenerationOrchestrator>();

        if (_track == null)
        {
            _track = null;
            _isDragging = false;
            return;
        }

        if (_track.GetRoot() == null || _track.startConnection == null)
        {
            _isDragging = false;
            return;
        }

        if (_mouseHeld && !_isDragging)
        {
            _isDragging = true;
        }
        else if (!_mouseHeld && _isDragging)
        {
            _track.TrySnap();
            _isDragging = false;
        }
    }
}
#endif
