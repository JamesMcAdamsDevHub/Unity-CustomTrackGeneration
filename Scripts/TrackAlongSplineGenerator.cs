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

    private const string START_ENDCAP_NAME = "Start_Endcap";
    private const string END_ENDCAP_NAME = "End_Endcap";

    private const string END_CONNECTION_ID = "End_Connection";
    protected override string ROOT_NAME => "Spline_Track_Root";

#if UNITY_EDITOR
    private bool _isEnforcingTangents;
#endif

    public override void ConnectionAttachedUpdate(string ID)
    {
        string targetEndcapName;

        if (ID == START_CONNECTION_ID && _generateStartEndcap)
        {
            targetEndcapName = START_ENDCAP_NAME;
        }
        else if (ID == END_CONNECTION_ID && _generateEndEndcap)
        {
            targetEndcapName = END_ENDCAP_NAME;
        }
        else
        {
            return;
        }
        DestroySpecifiedEndcap(targetEndcapName);
    }

    public override void ConnectionDettachedUpdate(string ID)
    {
        if (!_generateStartEndcap && !_generateEndEndcap) return;

        PopulateEndcapPoints();

        if (ID == START_CONNECTION_ID && _generateStartEndcap)
            GenerateSpecifiedEndcap(startEndcapPoint, START_ENDCAP_NAME);
        else if (ID == END_CONNECTION_ID && _generateEndEndcap)
            GenerateSpecifiedEndcap(endEndcapPoint, END_ENDCAP_NAME);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        Spline.Changed += OnSplineChanged;
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
    {
        if (_splineContainer == null) return;
        if (spline != _splineContainer.Spline) return;
        if (_isEnforcingTangents) return;

        EnforceAutoTangents();
    }
#endif

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

        Transform root = GetRoot();
        if (root == null) return;

        if (_generateStartEndcap && root.Find(START_ENDCAP_NAME) == null)
        {
            ConnectionPoint startPoint = GetLocalConnectionPointByID(START_CONNECTION_ID);

            if (startPoint == null || !startPoint.isConnected)
            {
                GenerateSpecifiedEndcap(startEndcapPoint, START_ENDCAP_NAME);
            }
        }

        if (_generateEndEndcap && root.Find(END_ENDCAP_NAME) == null)
        {
            ConnectionPoint endPoint = GetLocalConnectionPointByID(END_CONNECTION_ID);

            if (endPoint == null || !endPoint.isConnected)
            {
                GenerateSpecifiedEndcap(endEndcapPoint, END_ENDCAP_NAME);
            }
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
        LocalPointData newPoint = new LocalPointData();
        // Generate all track segments
        for (int i = 1; i <= numTracks; i++)
        {
            TrackRingsData trackRingsData = new TrackRingsData(_trackConstraintsData);
            nextMaxPosAlongSpline += splineDistancePerTrack;
            if (i == numTracks) nextMaxPosAlongSpline = 1f;
            float distanceFromLastRing = 0;
            newPoint = new LocalPointData();
            while (t <= nextMaxPosAlongSpline)
            {
                // Get local values at t along spline
                _splineContainer.Evaluate(t, out posTemp, out tanTemp, out upTemp);
                localPosition = transform.InverseTransformPoint((Vector3)posTemp);
                localForward = transform.InverseTransformDirection((Vector3)tanTemp).normalized;
                localUp = transform.InverseTransformDirection((Vector3)upTemp).normalized;
                newPoint = new LocalPointData(localPosition, localForward, localUp);

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
        GenerateConnectionPoint(newPoint, END_CONNECTION_ID);
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

    private void EnforceAutoTangents()
    {
        if (_splineContainer == null || _splineContainer.Spline == null) return;

#if UNITY_EDITOR
        if (_isEnforcingTangents) return;
        _isEnforcingTangents = true;
#endif

        try
        {
            Spline spline = _splineContainer.Spline;

            for (int i = 0; i < spline.Count; i++)
            {
                if (i == 0)
                {
                    spline.SetTangentMode(i, TangentMode.Broken);

                    BezierKnot knot = spline[i];
                    knot.Position = float3.zero;
                    knot.Rotation = quaternion.identity;
                    knot.TangentIn = float3.zero;
                    knot.TangentOut = new float3(0f, 0f, 1f);
                    spline.SetKnot(i, knot);
                }
                else
                {
                    spline.SetTangentMode(i, TangentMode.AutoSmooth);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(_splineContainer);
#endif
        }
        finally
        {
#if UNITY_EDITOR
            _isEnforcingTangents = false;
#endif
        }
    }
}