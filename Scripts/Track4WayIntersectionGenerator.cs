using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class Track4WayIntersectionGenerator : TrackGenerationOrchestrator
{
    private const string TRACK_OBJECT_NAME = "Track_Segment";
    private const string NORTH_CONNECTION_ID = "North_Connection";
    private const string EAST_CONNECTION_ID = "East_Connection";
    private const string WEST_CONNECTION_ID = "West_Connection";

    protected override string ROOT_NAME => "4_Way_Intersection_Root";

#if UNITY_EDITOR
    private bool _refreshQueued;
#endif

    public override void ConnectionAttachedUpdate(string ID)
    {
        RefreshIntersectionGeometry();
    }

    public override void ConnectionDetachedUpdate(string ID)
    {
        RefreshIntersectionGeometry();
    }

    protected override void StaleConnectionDetachedUpdate()
    {
        RefreshIntersectionGeometry();
    }

    protected override void GenerateNewTrack()
    {
        Transform root = GetRoot();
        if (root == null) return;
        if (root.Find(TRACK_OBJECT_NAME) != null) return;

#if UNITY_EDITOR
        GenerateConnectionPoint(
            new LocalPointData(
                Vector3.forward * _trackConstraintsData.TrackWidth,
                Vector3.forward,
                Vector3.up
            ),
            NORTH_CONNECTION_ID
        );

        GenerateConnectionPoint(
            new LocalPointData(
                new Vector3(_trackConstraintsData.TrackWidth / 2f, 0f, _trackConstraintsData.TrackWidth / 2f),
                Vector3.right,
                Vector3.up
            ),
            EAST_CONNECTION_ID
        );

        GenerateConnectionPoint(
            new LocalPointData(
                new Vector3(-_trackConstraintsData.TrackWidth / 2f, 0f, _trackConstraintsData.TrackWidth / 2f),
                Vector3.left,
                Vector3.up
            ),
            WEST_CONNECTION_ID
        );

        GenerateIntersectionGeometry(root);
#endif
    }

    private void RefreshIntersectionGeometry()
    {
#if UNITY_EDITOR
        if (Undo.isProcessing)
        {
            QueueRefreshIntersectionGeometry();
            return;
        }

        Transform root = GetRoot();
        if (root == null) return;

        if (_settings != null)
            _settings.CopyTo(_trackConstraintsData);

        Transform existingTrack = root.Find(TRACK_OBJECT_NAME);
        if (existingTrack != null)
            Undo.DestroyObjectImmediate(existingTrack.gameObject);

        GenerateIntersectionGeometry(root);
#endif
    }

#if UNITY_EDITOR
    private void QueueRefreshIntersectionGeometry()
    {
        if (_refreshQueued)
            return;

        _refreshQueued = true;
        EditorApplication.delayCall += () =>
        {
            if (this == null)
                return;

            _refreshQueued = false;
            RefreshIntersectionGeometry();
        };
    }
#endif

    private void GenerateIntersectionGeometry(Transform root)
    {
#if UNITY_EDITOR
        Track4WayIntersectionData intersectionData = new Track4WayIntersectionData(_trackConstraintsData);

        intersectionData.GenerateIntersectionData(
            IsConnectionOpen(START_CONNECTION_ID),
            IsConnectionOpen(NORTH_CONNECTION_ID),
            IsConnectionOpen(EAST_CONNECTION_ID),
            IsConnectionOpen(WEST_CONNECTION_ID)
        );

        TrackIntersection intersection = new TrackIntersection(
            "Track_4_Way_Intersection",
            _settings.deckMaterial,
            _settings.railMaterial,
            _settings.baseMaterial,
            intersectionData.deckMeshData,
            intersectionData.railMeshData,
            intersectionData.baseMeshData
        );

        GameObject intersectionGO = intersection.Generate();

        Undo.RegisterCreatedObjectUndo(intersectionGO, "Create 4-Way Intersection");
        Undo.SetTransformParent(intersectionGO.transform, root, "Attach 4-Way Intersection to root");
        intersectionGO.transform.localPosition = Vector3.zero;
        intersectionGO.transform.localRotation = Quaternion.identity;
        intersectionGO.transform.localScale = Vector3.one;
#endif
    }

    private bool IsConnectionOpen(string ID)
    {
        ConnectionPoint point = ID == START_CONNECTION_ID
            ? startConnection
            : GetLocalConnectionPointByID(ID);

        return point != null && point.isConnected;
    }

}
