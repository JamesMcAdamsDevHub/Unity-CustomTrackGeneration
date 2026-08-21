using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Track3WayIntersectionGenerator : TrackGenerationOrchestrator
{
    private const string TRACK_OBJECT_NAME = "Track_Segment";
    private const string LEFT_CONNECTION_ID = "Left_Connection";
    private const string RIGHT_CONNECTION_ID = "Right_Connection";

    protected override string ROOT_NAME => "3_Way_Intersection_Root";

    protected override void GenerateNewTrack()
    {
        Transform root = GetRoot();
        if (root == null) return;
        if (root.Find(TRACK_OBJECT_NAME) != null) return;

#if UNITY_EDITOR
        GenerateConnectionPoint(GetLeftConnectionPoint(), LEFT_CONNECTION_ID);
        GenerateConnectionPoint(GetRightConnectionPoint(), RIGHT_CONNECTION_ID);

        GenerateIntersectionGeometry(root);
#endif
    }

    private void GenerateIntersectionGeometry(Transform root)
    {
#if UNITY_EDITOR
        Track3WayIntersectionData intersectionData = new Track3WayIntersectionData(_trackConstraintsData);

        intersectionData.GenerateIntersectionData();

        TrackIntersection intersection = new TrackIntersection(
            "Track_3_Way_Intersection",
            _settings.deckMaterial,
            _settings.railMaterial,
            _settings.baseMaterial,
            intersectionData.deckMeshData,
            intersectionData.railMeshData,
            intersectionData.baseMeshData
        );

        GameObject intersectionGO = intersection.Generate();

        Undo.RegisterCreatedObjectUndo(intersectionGO, "Create 3-Way Intersection");
        Undo.SetTransformParent(intersectionGO.transform, root, "Attach 3-Way Intersection to root");
        intersectionGO.transform.localPosition = Vector3.zero;
        intersectionGO.transform.localRotation = Quaternion.identity;
        intersectionGO.transform.localScale = Vector3.one;
#endif
    }

    private LocalPointData GetLeftConnectionPoint()
    {
        Vector3 center = GetIntersectionCenter();
        Vector3 localPosition = center + Quaternion.AngleAxis(-120f, Vector3.up) * (Vector3.back * _trackConstraintsData.TrackWidth);
        Vector3 localForward = (localPosition - center).normalized;

        return new LocalPointData(localPosition, localForward, Vector3.up);
    }

    private LocalPointData GetRightConnectionPoint()
    {
        Vector3 center = GetIntersectionCenter();
        Vector3 localPosition = center + Quaternion.AngleAxis(120f, Vector3.up) * (Vector3.back * _trackConstraintsData.TrackWidth);
        Vector3 localForward = (localPosition - center).normalized;

        return new LocalPointData(localPosition, localForward, Vector3.up);
    }

    private Vector3 GetIntersectionCenter()
    {
        return Vector3.forward * _trackConstraintsData.TrackWidth;
    }

}
