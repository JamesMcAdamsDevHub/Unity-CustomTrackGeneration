using UnityEngine;

public class TrackLoopGenerator : TrackGenerationOrchestrator
{
    [Header("Loop Dimensions")]

    [SerializeField, Range(5f, 30f),
        Tooltip("Radius of the generated loop.")] 
    private float _radius = 5f;

    [SerializeField, Range(0.2f, 1f), 
        Tooltip("Normalized percentage of a full loop to generate.")] 
    private float _loopPercentage = 1f;

    [SerializeField, Range(0f, 50f), 
        Tooltip("Separation between the rising and falling sides of the loop. A value of 0 creates a circular loop. " +
        "Values above 0 offset the loop to avoid self-overlap.")] 
    private float _loopGap = 0.1f;

    [SerializeField, Range(0f, 0.5f),
        Tooltip("Normalized percentage of a track roll along loop.")]
    private float _embankment = 0.5f;

    [SerializeField]
    private bool _loopsRight = true;

    protected override string ROOT_NAME => "Loop_Root";

    protected override void GenerateNewTrack()
    {
        LocalPointData[] points = GenerateLoopPoints();
        TrackRingsData trackRingsData = new TrackRingsData(_trackConstraintsData);

        trackRingsData.GenerateRingAtPoint(points[0], 0f);

        for (int i = 1; i < points.Length; i++)
        {
            float distanceFromLastRing = Vector3.Distance(points[i - 1].localPosition, points[i].localPosition);
            trackRingsData.GenerateRingAtPoint(points[i], distanceFromLastRing);
        }

        LocalPointData endPoint = points[points.Length - 1];
        GenerateConnectionPoint(endPoint, END_CONNECTION_ID);
        CreateTrackSegment(trackRingsData);
    }

    private LocalPointData[] GenerateLoopPoints()
    {
        LocalPointData[] points = new LocalPointData[RINGS_PER_TRACK + 1];
        Vector3[] positions = new Vector3[points.Length];
        float[] pitchAngles = new float[points.Length];
        float lateralOffset = (_loopGap == 0f) ? 0f : _settings.trackWidth + _loopGap;
        if (!_loopsRight) lateralOffset *= -1;
        Vector3 rollOffset = GetRollOffset();
        float totalPitchAngle = GetTheta(_loopPercentage);
        float pathDistance = Mathf.Max(0.001f, Mathf.Abs(totalPitchAngle * _radius));
        float stepDistance = pathDistance / RINGS_PER_TRACK;
        float pitchScale = totalPitchAngle / GetLoopProfileIntegral(pathDistance);

        positions[0] = Vector3.zero;
        pitchAngles[0] = 0f;

        for (int ringIdx = 1; ringIdx < points.Length; ringIdx++)
        {
            float previousDistance = stepDistance * (ringIdx - 1);
            float currentDistance = stepDistance * ringIdx;
            float midDistance = (previousDistance + currentDistance) * 0.5f;

            pitchAngles[ringIdx] = pitchAngles[ringIdx - 1] + GetLoopProfile(midDistance, pathDistance) * pitchScale * stepDistance;
            positions[ringIdx] = positions[ringIdx - 1] + GetLocalForward(pitchAngles[ringIdx]) * stepDistance;
            positions[ringIdx].x = lateralOffset * GetLateralProgression(ringIdx);
        }

        for (int ringIdx = 0; ringIdx < points.Length; ringIdx++)
        {
            Vector3 localForward = GetLocalForwardFromPositions(positions, ringIdx);
            Vector3 localUp = GetLocalUp(localForward, pitchAngles[ringIdx], rollOffset);

            points[ringIdx] = new LocalPointData(positions[ringIdx], localForward, localUp);
        }

        return points;
    }

    private float GetLoopProgression(int ringIdx)
    {
        return (float)ringIdx / RINGS_PER_TRACK * _loopPercentage;
    }

    private float GetLateralProgression(int ringIdx)
    {
        return SmootherStep((float)ringIdx / RINGS_PER_TRACK) * _loopPercentage;
    }

    private float GetTheta(float progression)
    {
        return progression * 2f * Mathf.PI;
    }

    private Vector3 GetLocalForward(float theta)
    {
        return new Vector3(0f, Mathf.Sin(theta), Mathf.Cos(theta));
    }

    private Vector3 GetLocalForwardFromPositions(Vector3[] positions, int index)
    {
        Vector3 forward;

        if (index == 0)
            return Vector3.forward;
        else if (index == positions.Length - 1)
            forward = positions[index] - positions[index - 1];
        else
            forward = positions[index + 1] - positions[index - 1];

        return forward.sqrMagnitude <= 0.0001f ? Vector3.forward : forward.normalized;
    }

    private Vector3 GetRollOffset()
    {
        return new Vector3(_embankment * (_loopsRight ? 1 : -1), 0f, 0f);
    }

    private Vector3 GetLocalUp(Vector3 localForward, float theta, Vector3 rollOffset)
    {
        Vector3 loopUp = new Vector3(0f, Mathf.Cos(theta), -Mathf.Sin(theta));
        Vector3 rolledUp = loopUp + (Mathf.Sin(theta) * rollOffset);
        Vector3 projectedUp = Vector3.ProjectOnPlane(rolledUp, localForward);

        return projectedUp.sqrMagnitude <= 0.0001f ? Vector3.up : projectedUp.normalized;
    }

    private float GetLoopProfile(float distance, float pathDistance)
    {
        float smoothingDistance = GetAutoLoopSmoothingDistance(pathDistance);

        if (smoothingDistance <= 0f)
            return 1f;

        float fadeIn = SmootherStep(distance / smoothingDistance);
        float fadeOut = SmootherStep((pathDistance - distance) / smoothingDistance);

        return fadeIn * fadeOut;
    }

    private float GetLoopProfileIntegral(float pathDistance)
    {
        float total = 0f;

        for (int step = 0; step < RINGS_PER_TRACK; step++)
        {
            float distance = pathDistance * (step + 0.5f) / RINGS_PER_TRACK;
            total += GetLoopProfile(distance, pathDistance);
        }

        return Mathf.Max(0.001f, total / RINGS_PER_TRACK * pathDistance);
    }

    private float GetAutoLoopSmoothingDistance(float pathDistance)
    {
        if (pathDistance <= 0.001f)
            return 0f;

        float generatedRingSpacing = pathDistance / RINGS_PER_TRACK;
        float configuredRingSpacing = Mathf.Max(0f, _trackConstraintsData.DistanceBetweenRings);
        float ringSpacing = Mathf.Max(generatedRingSpacing, configuredRingSpacing);
        float ringBasedDistance = ringSpacing * 84f;
        float pathBasedDistance = pathDistance * 0.35f;
        float radiusBasedDistance = _radius * 1.5f;

        return Mathf.Min(pathDistance * 0.5f, Mathf.Max(pathBasedDistance, radiusBasedDistance, ringBasedDistance));
    }

    private float SmootherStep(float t)
    {
        float clampedT = Mathf.Clamp01(t);
        return clampedT * clampedT * clampedT * (clampedT * (clampedT * 6f - 15f) + 10f);
    }
}

