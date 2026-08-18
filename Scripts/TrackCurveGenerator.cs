using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackCurveGenerator : TrackGenerationOrchestrator
{
    [Header("Curve Dimensions")]

    [SerializeField, Range(5f, 30f),
        Tooltip("Radius of the generated curve.")] 
    private float _radius = 5f;

    [SerializeField, Range(0.2f, 1f), 
        Tooltip("Normalized percentage of a full curve to generate.")] 
    private float _curvePercentage = 0.5f;

    [SerializeField, Range(-30f, 30f),
        Tooltip("Height change between start and end of curve.")]
    private float _heightOffset = 0f;


    [SerializeField, Range(0f, 1f),
        Tooltip("Normalized percentage of a track roll along curve.")]
    private float _embankment = 1f;

    [SerializeField,
        Tooltip("If enabled, the curve banks back to neutral by the end. If disabled, the curve ends at full embankment.")]
    private bool _returnEmbankmentToNeutral = true;

    [SerializeField]
    private bool _loopsRight = true;

    protected override string ROOT_NAME => "Curve_Root";

    protected override void GenerateNewTrack()
    {
        TrackRingsData trackRingsData = new TrackRingsData(_trackConstraintsData);

        float distanceFromLastPosition = 0f;
        LocalPointData currentPoint = new LocalPointData();
        for (int ringIdx = 1; ringIdx <= RINGS_PER_TRACK; ringIdx++)
        {
            trackRingsData.GenerateRingAtPoint(currentPoint, distanceFromLastPosition);

            float progression = GetCurveProgression(ringIdx);
            float theta = GetTheta(progression);
            currentPoint.localPosition = GetCurvePosition(progression, theta);

            float nextProgression = GetCurveProgression(ringIdx + 1);
            float nextTheta = GetTheta(nextProgression);
            Vector3 nextPosition = GetCurvePosition(nextProgression, nextTheta);

            currentPoint.localForward = GetLocalForward(theta);
            currentPoint.localUp = GetLocalUp(currentPoint.localForward, progression);

            distanceFromLastPosition = Vector3.Distance(currentPoint.localPosition, nextPosition);
        }

        GenerateConnectionPoint(currentPoint, "End_Connection");

        trackRingsData.GenerateRingAtPoint(currentPoint, distanceFromLastPosition);
        CreateTrackSegment(trackRingsData);
    }

    private float GetCurveProgression(int ringIdx)
    {
        return (float)ringIdx / RINGS_PER_TRACK * _curvePercentage;
    }

    private float GetTheta(float progression)
    {
        return progression * 2f * Mathf.PI;
    }

    private Vector3 GetCurvePosition(float progression, float theta)
    {
        float curveRadius = GetCurveRadius();
        float turnDirection = _loopsRight ? 1f : -1f;
        float x = turnDirection * curveRadius * (1f - Mathf.Cos(theta));
        float y = _heightOffset * progression;
        float z = curveRadius * Mathf.Sin(theta);

        return new Vector3(x, y, z);
    }

    private Vector3 GetLocalForward(float theta)
    {
        float curveRadius = GetCurveRadius();
        float turnDirection = _loopsRight ? 1f : -1f;
        float x = turnDirection * curveRadius * Mathf.Sin(theta);
        float y = _heightOffset / (2f * Mathf.PI);
        float z = curveRadius * Mathf.Cos(theta);

        return new Vector3(x, y, z).normalized;
    }

    private float GetCurveRadius()
    {
        return _radius + _settings.trackWidth / 2f;
    }

    private Vector3 GetLocalUp(Vector3 localForward, float progression)
    {
        Vector3 localUp = Vector3.ProjectOnPlane(Vector3.up, localForward).normalized;
        float rollDirection = _loopsRight ? -1f : 1f;
        float normalizedProgression = Mathf.Clamp01(progression / _curvePercentage);
        float bankProfile = GetBankProfile(normalizedProgression);
        float bankAngle = _embankment * 90f * bankProfile * rollDirection;

        return (Quaternion.AngleAxis(bankAngle, localForward) * localUp).normalized;
    }

    private float GetBankProfile(float normalizedProgression)
    {
        if (_returnEmbankmentToNeutral)
            return Mathf.Sin(normalizedProgression * Mathf.PI);

        return Mathf.Sin(normalizedProgression * Mathf.PI * 0.5f);
    }
}

