#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;

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
            _track.hasBeenPlacedInScene = true;
            _track.TrySnap();
            _isDragging = false;

            if (_track is TrackAlongSplineGenerator)
            {
                TrackAlongSplineGenerator splineTrack = (TrackAlongSplineGenerator)_track;
                if (IsSelectedKnotLastInContainer(splineTrack))
                    splineTrack.hasConnectedLastSplineKnot = false;
                splineTrack.TryLastSplineKnotSnap();
                _track.ConnectAdjoiningPoints();
            }  

            Track4WayIntersectionGenerator.DisconnectStaleConnectionsForAllIntersections();
        }
    }
    private static bool IsSelectedKnotLastInContainer(TrackAlongSplineGenerator track)
    {
        SplineContainer container = track.GetComponent<SplineContainer>();
        if (container == null || container.Splines.Count == 0)
            return false;

        Spline spline = container.Splines[0];
        if (spline.Count == 0)
            return false;

        SplineInfo splineInfo = new SplineInfo(container, 0);
        ISelectableElement activeElement = SplineSelection.GetActiveElement(new[] { splineInfo });

        return activeElement != null && activeElement.KnotIndex == spline.Count - 1;
    }
}
#endif
