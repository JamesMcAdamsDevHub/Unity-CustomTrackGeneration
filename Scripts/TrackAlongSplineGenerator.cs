using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackAlongSplineGenerator : TrackGenerationOrchestrator
{
    [Header("Endcap Generation")]
    [SerializeField] private bool _generateStartEndcap;
    [SerializeField] private bool _generateEndEndcap;

    [SerializeField] private TrackGenerationSettings _settings = new();

    [Header("Spline Container")]
    [SerializeField] private SplineContainer _splineContainer;

    private TrackConstraintsData _trackConstraintsData = new TrackConstraintsData();
    private const string ROOT_NAME = "Track_Root";

    private void OnValidate()
    {
        if (_settings != null)
        {
            _settings.UpdateConstraints(this);
        }
    }
     
    public override void GenerateTrack()
    {
        if (_settings == null) return;

        _settings.CopyTo(_trackConstraintsData);
        GenerateNewTrack();
    }

    public override void RefreshFromConfig()
    {
        if (_settings != null)
        {
            _settings.UpdateConstraints(this);
        }
    }

    private Transform GetOrCreateRoot()
    {
        Transform existing = transform.Find(ROOT_NAME);
        if (existing != null)
            return existing;

        GameObject root = new GameObject(ROOT_NAME);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(root, "Create Track Root");
        Undo.SetTransformParent(root.transform, transform, "Attach root to parent");
#endif

        return root.transform;
    }

    private void GenerateNewTrack()
    {
        if (_splineContainer == null)
        {
            Debug.LogWarning("Could not generate track: SplineContainer not assigned.", this);
            return;
        }

        DestroyPreviousTrack();
        GenerateEndcaps();
        GenerateTrackAlongSpline();
    }

    private void DestroyPreviousTrack()
    {
        Transform root = transform.Find(ROOT_NAME);

        if (root == null) return;

#if UNITY_EDITOR
        Undo.DestroyObjectImmediate(root.gameObject);
#endif
    }


    private void GenerateEndcaps()
    {
        if (_generateStartEndcap)
        {
            GenerateSpecifiedEndcap(true);
        }

        if (_generateEndEndcap)
        {
            GenerateSpecifiedEndcap(false);
        }
    }

    private void GenerateSpecifiedEndcap(bool isStartEndcap)
    {
        float3 posTemp, tanTemp, upTemp;
        float t = isStartEndcap ? 0f : 1f;
        _splineContainer.Evaluate(t, out posTemp, out tanTemp, out upTemp);

        Vector3 localPosition = (Vector3)posTemp;
        Vector3 localForward = ((Vector3)tanTemp).normalized;
        Vector3 localUp = ((Vector3)upTemp).normalized;

        if (!isStartEndcap) localForward *= -1f;
#if UNITY_EDITOR
        Quaternion localRotation = Quaternion.LookRotation(localForward, localUp);
        TrackEndcapData endcapData = new TrackEndcapData(_trackConstraintsData);
        endcapData.GenerateEndcapData();
        TrackEndcap endcap = new TrackEndcap(_settings.railMaterial, _settings.baseMaterial, endcapData.railMeshData, endcapData.baseMeshData);
        GameObject endcapGO = endcap.Generate(localPosition, localRotation);
        Undo.RegisterCreatedObjectUndo(endcapGO, "Create Endcap");
        Transform root = GetOrCreateRoot();
        Undo.SetTransformParent(endcapGO.transform, root, "Attach endcap to root");
#endif
    }

    private void GenerateTrackAlongSpline()
    {
        const int MAX_VERTS_PER_TRACK = 6000;
        const int VERTS_PER_RING = 20;
        const int RINGS_PER_TRACK = MAX_VERTS_PER_TRACK / VERTS_PER_RING;
        float maxTrackLength = (float)RINGS_PER_TRACK * _settings.distanceBetweenRings;
        float splineLength = _splineContainer.Spline.GetLength();
        int numTracks = (int)(splineLength / maxTrackLength) + 1;
        float splineDistancePerTrack = 1f / numTracks;
        float t = 0f;
        float3 posTemp, tanTemp, upTemp;
        Vector3 localPosition = Vector3.zero;
        Vector3 localForward = Vector3.forward;
        Vector3 localUp = Vector3.up;
        float nextMaxPosAlongSpline = 0;

        // Generate all track segments
        for (int i = 1; i <= numTracks; i++)
        {
            TrackRingsData trackRingsData = new TrackRingsData(_trackConstraintsData);
            nextMaxPosAlongSpline += splineDistancePerTrack;
            if (i == numTracks) nextMaxPosAlongSpline = 1f;
            float distanceFromLastRing = 0;
            while (t <= nextMaxPosAlongSpline)
            {
                // Get local values at t along spline
                _splineContainer.Evaluate(t, out posTemp, out tanTemp, out upTemp);
                localPosition = (Vector3)posTemp;
                localForward = ((Vector3)tanTemp).normalized;
                localUp = ((Vector3)upTemp).normalized;

                // Generate MeshData in trackRingsData at t
                trackRingsData.GenerateRingAtPoint(localPosition, localForward, localUp, distanceFromLastRing);

                if (t >= nextMaxPosAlongSpline) break;

                Vector3 prevPos = _splineContainer.EvaluatePosition(t);
                // Get next position along spline
                t = GetNextPosAlongSpline(t, nextMaxPosAlongSpline);

                Vector3 currPos = _splineContainer.EvaluatePosition(t);
                distanceFromLastRing = Vector3.Distance(prevPos, currPos);
            }
#if UNITY_EDITOR
            TrackSegment trackSegment = new TrackSegment(_settings.deckMaterial, _settings.railMaterial, _settings.baseMaterial, trackRingsData.deckMeshData, trackRingsData.railMeshData, trackRingsData.baseMeshData);
            Quaternion localRotation = Quaternion.LookRotation(localForward, localUp);
            GameObject trackSegmentGO = trackSegment.Generate();
            Undo.RegisterCreatedObjectUndo(trackSegmentGO, "Create Track Segment");
            Transform root = GetOrCreateRoot();
            Undo.SetTransformParent(trackSegmentGO.transform, root, "Attach trackSegment to root");
#endif
        }
    }

    private float GetNextPosAlongSpline(float t, float maxPosAlongSpline)
    {
        const float INCREMENT_VAL = 0.01f;
        Vector3 currPos = _splineContainer.EvaluatePosition(t), nextPos;
        float distanceToNextPos;
        do
        {
            float nextT = Mathf.Min(t + INCREMENT_VAL, maxPosAlongSpline);
            nextPos = _splineContainer.EvaluatePosition(nextT);
            distanceToNextPos = Vector3.Distance(currPos, nextPos);
            t = nextT;
        }
        while (t < maxPosAlongSpline && distanceToNextPos < _settings.distanceBetweenRings);

        if (t > maxPosAlongSpline) return maxPosAlongSpline;

        return t;
    }
}