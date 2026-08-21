using UnityEngine;

public class TrackCurveGenerator : TrackGenerationOrchestrator
{
    private const float FULL_TURN_DEGREES = 360f;

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

    [SerializeField,
        Tooltip("Keeps the curve at a constant radius for predictable modular alignment. Disable to ease into and out of the turn.")]
    private bool _useConstantRadius = false;

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

        LocalPointData[] points = GenerateCurvePoints();
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

    private LocalPointData[] GenerateCurvePoints()
    {
        return _useConstantRadius
            ? GenerateExactArcPoints()
            : GenerateSmoothedCurvePoints();
    }

    private LocalPointData[] GenerateExactArcPoints()
    {
        LocalPointData[] points = new LocalPointData[RINGS_PER_TRACK + 1];
        float signedArcAngle = GetSignedArcAngleRadians();
        float horizontalDistance = GetHorizontalPathDistance();

        for (int i = 0; i < points.Length; i++)
        {
            float t = (float)i / RINGS_PER_TRACK;
            float angle = signedArcAngle * t;
            Vector3 localPosition = GetExactArcPosition(angle, t);
            Vector3 localForward = GetForwardAtAngle(angle, t, horizontalDistance);
            Vector3 localUp = GetBankedLocalUp(localForward, GetBankProfile(t));

            points[i] = new LocalPointData(localPosition, localForward, localUp);
        }

        return points;
    }

    private LocalPointData[] GenerateSmoothedCurvePoints()
    {
        LocalPointData[] points = new LocalPointData[RINGS_PER_TRACK + 1];
        float horizontalDistance = GetHorizontalPathDistance();
        float stepDistance = horizontalDistance / RINGS_PER_TRACK;
        float curvatureScale = GetCurvatureScale(horizontalDistance);
        float totalPathDistance = GetTotalPathDistance();
        Vector3 pathPosition = Vector3.zero;
        float yaw = 0f;

        points[0] = GetPointAt(Vector3.zero, 0f, 0f, horizontalDistance);

        for (int i = 1; i < points.Length; i++)
        {
            float previousDistance = stepDistance * (i - 1);
            float currentDistance = stepDistance * i;
            float midDistance = (previousDistance + currentDistance) * 0.5f;

            yaw += GetCurveProfile(midDistance, horizontalDistance) * curvatureScale * stepDistance;
            pathPosition += new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw)) * stepDistance;

            float t = (float)i / RINGS_PER_TRACK;
            pathPosition.y = _elevationOffset * SmootherStep(t);

            points[i] = GetPointAt(pathPosition, yaw, t, horizontalDistance);
        }

        return points;
    }

    private LocalPointData GetPointAt(Vector3 pathPosition, float yaw, float t, float horizontalDistance)
    {
        Vector3 pathForward = GetForwardAtAngle(yaw, t, horizontalDistance);
        Vector3 pathUp = GetBankedLocalUp(pathForward, GetBankProfile(t));
        Vector3 localPosition = pathPosition + GetBankedCenterLift(pathForward, pathUp);

        return new LocalPointData(localPosition, pathForward, pathUp);
    }

    private Vector3 GetExactArcPosition(float angle, float t)
    {
        float signedArcAngle = GetSignedArcAngleRadians();

        if (Mathf.Abs(signedArcAngle) <= 0.001f)
            return new Vector3(0f, _elevationOffset * SmootherStep(t), GetHorizontalPathDistance() * t);

        float turnDirection = GetTurnDirection();
        float x = turnDirection * _curveRadius * (1f - Mathf.Cos(angle));
        float z = _curveRadius * Mathf.Sin(Mathf.Abs(angle));

        return new Vector3(x, _elevationOffset * SmootherStep(t), z);
    }

    private float GetCurvatureScale(float horizontalDistance)
    {
        if (Mathf.Approximately(horizontalDistance, 0f))
            return 0f;

        return GetSignedArcAngleRadians() / GetCurveProfileIntegral(horizontalDistance);
    }

    private float GetCurveProfileIntegral(float horizontalDistance)
    {
        float total = 0f;

        for (int step = 0; step < RINGS_PER_TRACK; step++)
        {
            float distance = horizontalDistance * (step + 0.5f) / RINGS_PER_TRACK;
            total += GetCurveProfile(distance, horizontalDistance);
        }

        return Mathf.Max(0.001f, total / RINGS_PER_TRACK * horizontalDistance);
    }

    private float GetTotalPathDistance()
    {
        float horizontalDistance = GetHorizontalPathDistance();
        return Mathf.Max(0.001f, Mathf.Sqrt(horizontalDistance * horizontalDistance + _elevationOffset * _elevationOffset));
    }

    private float GetHorizontalPathDistance()
    {
        float turnAngleRadians = GetArcAngleRadians();

        if (Mathf.Abs(turnAngleRadians) <= 0.001f)
            return 0f;

        return Mathf.Abs(turnAngleRadians * _curveRadius);
    }

    private Vector3 GetForwardAtAngle(float angleRadians, float t, float horizontalDistance)
    {
        Vector3 flatForward = new Vector3(Mathf.Sin(angleRadians), 0f, Mathf.Cos(angleRadians));
        float verticalSlope = horizontalDistance <= 0.001f
            ? 0f
            : _elevationOffset * SmootherStepDerivative(t) / horizontalDistance;

        return new Vector3(flatForward.x, verticalSlope, flatForward.z).normalized;
    }

    private float GetCurveProfile(float distance, float horizontalDistance)
    {
        if (_useConstantRadius)
            return 1f;

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
        if (horizontalDistance <= 0.001f || turnAngleRadians <= 0.001f)
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

    private Vector3 GetBankedCenterLift(Vector3 localForward, Vector3 localUp)
    {
        Vector3 trackWidthFromCenter = Vector3.Cross(localForward, localUp).normalized * (_trackConstraintsData.TrackWidth / 2f);
        float loweredEdgeOffset = Mathf.Abs(trackWidthFromCenter.y);

        return Vector3.up * loweredEdgeOffset;
    }

    private float GetBankProfile(float t)
    {
        if (_maxBankDegrees <= 0f)
            return 0f;

        return GetCurveStrength() * GetBankBlendProfile(t);
    }

    private float GetBankBlendProfile(float t)
    {
        float totalPathDistance = GetTotalPathDistance();
        float blendDistance = GetAutoBankBlendDistance(totalPathDistance);

        if (blendDistance <= 0f)
            return 1f;

        float distance = Mathf.Clamp01(t) * totalPathDistance;
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

    private float GetBankSeverity()
    {
        return Mathf.Clamp01(_maxBankDegrees / 90f);
    }

    private float GetArcAngleRadians()
    {
        return _arcAngle * Mathf.Deg2Rad;
    }

    private float GetSignedArcAngleRadians()
    {
        return GetTurnDirection() * GetArcAngleRadians();
    }

    private float GetBankRollDirection()
    {
        return -GetTurnDirection();
    }

    private float GetTurnDirection()
    {
        return _curvesRight ? 1f : -1f;
    }

    private float SmootherStep(float t)
    {
        float clampedT = Mathf.Clamp01(t);
        return clampedT * clampedT * clampedT * (clampedT * (clampedT * 6f - 15f) + 10f);
    }

    private float SmootherStepDerivative(float t)
    {
        float clampedT = Mathf.Clamp01(t);
        return 30f * clampedT * clampedT * (clampedT - 1f) * (clampedT - 1f);
    }

    private void NormalizeControls()
    {
        if (_arcAngle < 0f)
            _curvesRight = false;

        _arcAngle = Mathf.Clamp(Mathf.Abs(_arcAngle), 1f, FULL_TURN_DEGREES);
    }
}
