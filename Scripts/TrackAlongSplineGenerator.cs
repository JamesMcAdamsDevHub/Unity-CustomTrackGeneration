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

    [Header("Spline Container")]
    [SerializeField] private SplineContainer _splineContainer;

    private LocalPointData startEndcapPoint = new LocalPointData();
    private LocalPointData endEndcapPoint = new LocalPointData();

    protected override string ROOT_NAME => "Spline_Track_Root";

    protected override void GenerateNewTrack()
    {
        if (_splineContainer == null)
        {
            Debug.LogWarning("Could not generate track: SplineContainer not assigned.", this);
            return;
        }

        GenerateTrackAlongSpline();
        GenerateEndcaps();
    }

    private void GenerateEndcaps()
    {
        PopulateEndcapPoints();

        if (_generateStartEndcap)
        {
            GenerateSpecifiedEndcap(startEndcapPoint);
        }

        if (_generateEndEndcap)
        {
            GenerateSpecifiedEndcap(endEndcapPoint);
        }
}

    private void PopulateEndcapPoints()
    {
        if (_splineContainer == null)
        {
            Debug.LogWarning("Could not generate track: SplineContainer not assigned.", this);
            return;
        }

        float3 posTemp, tanTemp, upTemp;
        Vector3 localPosition, localForward, localUp;

        _splineContainer.Evaluate(0, out posTemp, out tanTemp, out upTemp);

        localPosition = (Vector3)posTemp;
        localForward = ((Vector3)tanTemp).normalized;
        localUp = ((Vector3)upTemp).normalized;

        startEndcapPoint.localPosition = localPosition;
        startEndcapPoint.localForward = localForward;
        startEndcapPoint.localUp = localUp;

        _splineContainer.Evaluate(1, out posTemp, out tanTemp, out upTemp);

        localPosition = (Vector3)posTemp;
        localForward = ((Vector3)tanTemp).normalized * -1;
        localUp = ((Vector3)upTemp).normalized;

        endEndcapPoint.localPosition = localPosition;
        endEndcapPoint.localForward = localForward;
        endEndcapPoint.localUp = localUp;
    }

    private void GenerateTrackAlongSpline() 
    {
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
                localPosition = transform.InverseTransformPoint((Vector3)posTemp);
                localForward = transform.InverseTransformDirection((Vector3)tanTemp).normalized;
                localUp = transform.InverseTransformDirection((Vector3)upTemp).normalized;
                LocalPointData newPoint = new LocalPointData(localPosition, localForward, localUp);

                // Generate MeshData in trackRingsData at t
                trackRingsData.GenerateRingAtPoint(newPoint, distanceFromLastRing);

                if (t >= nextMaxPosAlongSpline) break;

                Vector3 prevPos = _splineContainer.EvaluatePosition(t);

                // Get next position along spline
                t = GetNextPosAlongSpline(t, nextMaxPosAlongSpline);

                Vector3 currPos = _splineContainer.EvaluatePosition(t);
                distanceFromLastRing = Vector3.Distance(prevPos, currPos);
            }

            CreateTrackSegment(trackRingsData);
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