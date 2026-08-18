using UnityEngine;

public class TrackCurveGenerator : TrackGenerationOrchestrator
{
    private const int INTEGRATION_STEPS = 360;
    private const float FULL_TURN_DEGREES = 360f;
    private const float TANGENT_SAMPLE_STEP = 0.0025f;
    private const string END_CONNECTION_ID = "End_Connection";

    [Header("Primary Controls")]

    [SerializeField, Range(5f, 50f),
        Tooltip("Radius of the curve.")]
    private float _curveRadius = 10f;

    [SerializeField, Range(15f, 360f),
        Tooltip("Arc angle in degrees. 360 creates a full loop.")]
    private float _arcAngle = 90f;

    [SerializeField,
        Tooltip("When enabled, the curve turns right. When disabled, it turns left.")]
    private bool _curvesRight = true;

    [SerializeField, Range(-30f, 30f),
        Tooltip("Elevation offset between start and end.")]
    private float _elevationOffset = 0f;

    [SerializeField, Range(0f, 90f),
        Tooltip("Maximum road tilt in degrees. Actual bank is automatically scaled from curve shape.")]
    private float _maxBankDegrees = 20f;

    [SerializeField,
        Tooltip("When enabled, bank eases back to flat at the end. When disabled, the end remains banked for another piece to continue.")]
    private bool _returnBankToFlat = true;

    protected override string ROOT_NAME => "Curve_Root";

    protected override void OnValidate()
    {
        base.OnValidate();
        NormalizeControls();
    }

    protected override void GenerateNewTrack()
    {
        NormalizeControls();

        TrackRingsData trackRingsData = new TrackRingsData(_trackConstraintsData);
        float totalPathDistance = GetTotalPathDistance();

        float distanceFromLastPosition = 0f;
        LocalPointData currentPoint = GetLocalPointAtDistance(0f);
        for (int ringIdx = 1; ringIdx <= RINGS_PER_TRACK; ringIdx++)
        {
            trackRingsData.GenerateRingAtPoint(currentPoint, distanceFromLastPosition);

            LocalPointData nextPoint = GetLocalPointAtDistance(GetCurveDistance(ringIdx, totalPathDistance));
            distanceFromLastPosition = Vector3.Distance(currentPoint.localPosition, nextPoint.localPosition);
            currentPoint = nextPoint;
        }

        GenerateConnectionPoint(currentPoint, END_CONNECTION_ID);

        trackRingsData.GenerateRingAtPoint(currentPoint, distanceFromLastPosition);
        CreateTrackSegment(trackRingsData);
    }

    private float GetCurveDistance(int ringIdx, float totalPathDistance)
    {
        return (float)ringIdx / RINGS_PER_TRACK * totalPathDistance;
    }

    private float GetTotalPathDistance()
    {
        float horizontalDistance = GetHorizontalPathDistance();
        float verticalDistance = _elevationOffset;

        return Mathf.Max(0.001f, Mathf.Sqrt(horizontalDistance * horizontalDistance + verticalDistance * verticalDistance));
    }

    private LocalPointData GetLocalPointAtDistance(float distance)
    {
        float totalPathDistance = GetTotalPathDistance();
        float clampedDistance = Mathf.Clamp(distance, 0f, totalPathDistance);

        if (Mathf.Approximately(clampedDistance, 0f))
            return new LocalPointData(Vector3.zero, Vector3.forward, Vector3.up);

        float t = Mathf.Approximately(totalPathDistance, 0f) ? 1f : clampedDistance / totalPathDistance;
        Vector3 localPosition = GetLocalPosition(t);
        Vector3 localForward = GetLocalForward(t);
        Vector3 localUp = GetBankedLocalUp(localForward, GetBankProfile(clampedDistance, totalPathDistance));

        return new LocalPointData(localPosition, localForward, localUp);
    }

    private Vector3 GetLocalPosition(float t)
    {
        Vector3 pathPosition = GetPathPosition(t);
        Vector3 pathForward = GetPathForward(t);
        Vector3 pathUp = GetBankedLocalUp(pathForward, GetBankProfileAtT(t));

        return pathPosition + GetBankedCenterLift(pathForward, pathUp);
    }

    private Vector3 GetPathPosition(float t)
    {
        t = Mathf.Clamp01(t);
        Vector3 flatPosition = GetIntegratedFlatPosition(t);
        return new Vector3(flatPosition.x, _elevationOffset * SmootherStep(t), flatPosition.z);
    }

    private float SmootherStep(float t)
    {
        float clampedT = Mathf.Clamp01(t);
        return clampedT * clampedT * clampedT * (clampedT * (clampedT * 6f - 15f) + 10f);
    }

    private Vector3 GetLocalForward(float t)
    {
        float clampedT = Mathf.Clamp01(t);
        float previousT = Mathf.Clamp01(clampedT - TANGENT_SAMPLE_STEP);
        float nextT = Mathf.Clamp01(clampedT + TANGENT_SAMPLE_STEP);

        if (Mathf.Approximately(previousT, nextT))
            return GetPathForward(clampedT);

        Vector3 tangent = GetLocalPosition(nextT) - GetLocalPosition(previousT);
        if (tangent.sqrMagnitude <= 0.000001f)
            return GetPathForward(clampedT);

        return tangent.normalized;
    }

    private Vector3 GetPathForward(float t)
    {
        float clampedT = Mathf.Clamp01(t);

        if (clampedT <= TANGENT_SAMPLE_STEP)
            return GetForwardAtAngle(0f, 0f);

        if (clampedT >= 1f - TANGENT_SAMPLE_STEP)
            return GetForwardAtAngle(GetSignedArcAngleRadians(), 0f);

        float previousT = Mathf.Clamp01(clampedT - TANGENT_SAMPLE_STEP);
        float nextT = Mathf.Clamp01(clampedT + TANGENT_SAMPLE_STEP);

        if (Mathf.Approximately(previousT, nextT))
            return GetForwardAtAngle(GetSignedArcAngleRadians(), GetTotalPathDistance());

        Vector3 tangent = GetPathPosition(nextT) - GetPathPosition(previousT);
        if (tangent.sqrMagnitude <= 0.000001f)
            return GetForwardAtAngle(GetSignedArcAngleRadians(), GetTotalPathDistance());

        return tangent.normalized;
    }

    private Vector3 GetForwardAtAngle(float angleRadians, float totalPathDistance)
    {
        Vector3 flatForward = new Vector3(Mathf.Sin(angleRadians), 0f, Mathf.Cos(angleRadians));
        float verticalSlope = Mathf.Approximately(totalPathDistance, 0f) ? 0f : _elevationOffset / totalPathDistance;

        return new Vector3(flatForward.x, verticalSlope, flatForward.z).normalized;
    }

    private float GetArcAngleRadians()
    {
        return _arcAngle * Mathf.Deg2Rad;
    }

    private float GetSignedArcAngleRadians()
    {
        return GetTurnDirection() * GetArcAngleRadians();
    }

    private Vector3 GetIntegratedFlatPosition(float t)
    {
        float clampedT = Mathf.Clamp01(t);
        float horizontalDistance = GetHorizontalPathDistance();
        float turnAngleRadians = GetSignedArcAngleRadians();
        float curvatureScale = Mathf.Approximately(horizontalDistance, 0f)
            ? 0f
            : turnAngleRadians / GetCurveProfileIntegral(horizontalDistance);
        Vector3 localPosition = Vector3.zero;
        float yaw = 0f;
        float previousDistance = 0f;
        float targetDistance = horizontalDistance * clampedT;
        int steps = Mathf.Max(1, Mathf.CeilToInt(INTEGRATION_STEPS * clampedT));

        for (int step = 1; step <= steps; step++)
        {
            float currentDistance = targetDistance * step / steps;
            float midDistance = (previousDistance + currentDistance) * 0.5f;
            float stepDistance = currentDistance - previousDistance;

            yaw += GetCurveProfile(midDistance, horizontalDistance) * curvatureScale * stepDistance;
            localPosition += new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw)) * stepDistance;
            previousDistance = currentDistance;
        }

        return localPosition;
    }

    private float GetCurveProfileIntegral(float horizontalDistance)
    {
        float total = 0f;

        for (int step = 0; step < INTEGRATION_STEPS; step++)
        {
            float distance = horizontalDistance * (step + 0.5f) / INTEGRATION_STEPS;
            total += GetCurveProfile(distance, horizontalDistance);
        }

        return Mathf.Max(0.001f, total / INTEGRATION_STEPS * horizontalDistance);
    }

    private float GetCurveProfile(float distance, float horizontalDistance)
    {
        float smoothingDistance = GetAutoCurveSmoothingDistance(horizontalDistance);

        if (smoothingDistance <= 0f)
            return 1f;

        float fadeIn = SmootherStep(distance / smoothingDistance);
        float fadeOut = SmootherStep((horizontalDistance - distance) / smoothingDistance);

        return fadeIn * fadeOut;
    }

    private float GetAutoCurveSmoothingDistance(float horizontalDistance)
    {
        float turnAngleRadians = GetArcAngleRadians();
        if (horizontalDistance <= 0.001f || turnAngleRadians <= 0.001f || _maxBankDegrees <= 0f)
            return 0f;

        float radius = horizontalDistance / Mathf.Max(0.001f, turnAngleRadians);
        float bankSeverity = GetBankSeverity();
        float ringBasedDistance = GetRingBasedTransitionDistance(
            horizontalDistance,
            Mathf.Lerp(24f, 84f, bankSeverity));
        float pathBasedDistance = horizontalDistance * Mathf.Lerp(0.3f, 0.5f, bankSeverity);
        float radiusBasedDistance = radius * Mathf.Lerp(0.8f, 2.25f, bankSeverity);
        float bankGeometryDistance = GetBankGeometryTransitionDistance() * 0.75f;

        return Mathf.Min(
            horizontalDistance * 0.5f,
            Mathf.Max(pathBasedDistance, radiusBasedDistance, ringBasedDistance, bankGeometryDistance));
    }

    private float GetHorizontalPathDistance()
    {
        float turnAngleRadians = GetArcAngleRadians();

        if (Mathf.Abs(turnAngleRadians) <= 0.001f)
            return 0f;

        return Mathf.Abs(turnAngleRadians * _curveRadius);
    }

    private Vector3 GetFlatLocalUp(Vector3 localForward)
    {
        return Vector3.ProjectOnPlane(Vector3.up, localForward).normalized;
    }

    private Vector3 GetBankedLocalUp(Vector3 localForward, float bankProfile)
    {
        Vector3 localUp = GetFlatLocalUp(localForward);
        float rollDirection = GetBankRollDirection();
        float bankAngle = _maxBankDegrees * bankProfile * rollDirection;

        return (Quaternion.AngleAxis(bankAngle, localForward) * localUp).normalized;
    }

    private float GetBankRollDirection()
    {
        return -GetTurnDirection();
    }

    private float GetTurnDirection()
    {
        return _curvesRight ? 1f : -1f;
    }

    private Vector3 GetBankedCenterLift(Vector3 localForward, Vector3 localUp)
    {
        Vector3 trackWidthFromCenter = Vector3.Cross(localForward, localUp).normalized * (_trackConstraintsData.TrackWidth / 2f);
        float loweredEdgeOffset = Mathf.Abs(trackWidthFromCenter.y);

        return Vector3.up * loweredEdgeOffset;
    }

    private float GetBankProfile(float distance, float totalPathDistance)
    {
        if (_maxBankDegrees <= 0f)
            return 0f;

        if (distance <= 0f)
            return 0f;

        return GetCurveStrength() * GetBankBlendProfile(distance, totalPathDistance);
    }

    private float GetBankProfileAtT(float t)
    {
        float totalPathDistance = GetTotalPathDistance();
        return GetBankProfile(Mathf.Clamp01(t) * totalPathDistance, totalPathDistance);
    }

    private float GetBankBlendProfile(float distance, float totalPathDistance)
    {
        float blendDistance = GetAutoBankBlendDistance(totalPathDistance);

        if (blendDistance <= 0f)
            return 1f;

        float blendIn = SmootherStep(distance / blendDistance);
        float blendOut = _returnBankToFlat
            ? SmootherStep((totalPathDistance - distance) / blendDistance)
            : 1f;

        return Mathf.Min(blendIn, blendOut);
    }

    private float GetAutoBankBlendDistance(float totalPathDistance)
    {
        if (totalPathDistance <= 0.001f || _maxBankDegrees <= 0f)
            return 0f;

        float bankSeverity = GetBankSeverity();
        float ringBasedDistance = GetRingBasedTransitionDistance(
            totalPathDistance,
            Mathf.Lerp(36f, 120f, bankSeverity));
        float pathBasedDistance = totalPathDistance * Mathf.Lerp(0.38f, 0.5f, bankSeverity);
        float shapeBasedDistance = Mathf.Max(_curveRadius * Mathf.Lerp(0.8f, 2.25f, bankSeverity), GetBankGeometryTransitionDistance());

        return Mathf.Min(totalPathDistance * 0.5f, Mathf.Max(pathBasedDistance, shapeBasedDistance, ringBasedDistance));
    }

    private float GetBankSeverity()
    {
        return Mathf.Clamp01(_maxBankDegrees / 90f);
    }

    private float GetRingBasedTransitionDistance(float pathDistance, float transitionRings)
    {
        if (pathDistance <= 0.001f)
            return 0f;

        float generatedRingSpacing = pathDistance / RINGS_PER_TRACK;
        float configuredRingSpacing = Mathf.Max(0f, _trackConstraintsData.DistanceBetweenRings);
        float ringSpacing = Mathf.Max(generatedRingSpacing, configuredRingSpacing);

        return ringSpacing * transitionRings;
    }

    private float GetBankGeometryTransitionDistance()
    {
        if (_maxBankDegrees <= 0f)
            return 0f;

        float bankRadians = Mathf.Clamp(_maxBankDegrees, 0f, 89f) * Mathf.Deg2Rad;
        float raisedEdgeTravel = Mathf.Tan(bankRadians) * _trackConstraintsData.TrackWidth * 0.5f;

        return Mathf.Max(_trackConstraintsData.TrackWidth, raisedEdgeTravel * 2f);
    }

    private float GetCurveStrength()
    {
        const float FULL_BANK_RADIUS = 15f;
        const float NO_BANK_RADIUS = 80f;

        return Mathf.InverseLerp(NO_BANK_RADIUS, FULL_BANK_RADIUS, _curveRadius);
    }

    private void NormalizeControls()
    {
        if (_arcAngle < 0f)
            _curvesRight = false;

        _arcAngle = Mathf.Clamp(Mathf.Abs(_arcAngle), 1f, FULL_TURN_DEGREES);
    }
}

