using UnityEngine;
using UnityEngine.Splines;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackLoopGenerator : TrackGenerationOrchestrator
{
    [Header("Loop Dimensions")]

    [SerializeField, Range(50f, 300f),
        Tooltip("Radius of the generated loop.")] 
    private float _radius = 50f;

    [SerializeField, Range(0.2f, 1f), 
        Tooltip("Normalized percentage of a full loop to generate.")] 
    private float _loopPercentage = 1f;

    [SerializeField, Range(0f, 500f), 
        Tooltip("Separation between the rising and falling sides of the loop. A value of 0 creates a circular loop. " +
        "Values above 0 offset the loop to avoid self-overlap.")] 
    private float _loopGap = 0.1f;

    [SerializeField, Range(0f, 0.5f),
        Tooltip("Normalized percentage of a track roll along loop.")]
    private float _embankment = 0.5f;

    protected override string ROOT_NAME => "Loop_Root";

    protected override void GenerateNewTrack()
    {
        TrackRingsData trackRingsData = new TrackRingsData(_trackConstraintsData);
        float lateralOffset = (_loopGap == 0f) ? 0f : _settings.trackWidth + _loopGap;
        Vector3 rollOffset = GetRollOffset();

        float distanceFromLastPosition = 0f;
        Vector3 currentPosition = Vector3.zero;
        Vector3 localForward = Vector3.forward;
        Vector3 localUp = Vector3.up;
        for (int ringIdx = 1; ringIdx <= RINGS_PER_TRACK; ringIdx++)
        {
            AddRingAtPoint(trackRingsData, currentPosition, localForward, localUp, distanceFromLastPosition);

            float progression = GetProgressionAlongLoop(ringIdx);
            float theta = GetTheta(progression);
            currentPosition = GetLoopPosition(lateralOffset, progression, theta);

            float nextProgression = GetProgressionAlongLoop(ringIdx + 1);
            float nextTheta = GetTheta(nextProgression);
            Vector3 nextPosition = GetLoopPosition(lateralOffset, nextProgression, nextTheta);

            localForward = GetLocalForward(theta);
            Vector3 loopCenter = GetLoopCenterAtX(currentPosition.x);
            localUp = GetLocalUp(currentPosition, loopCenter, rollOffset, theta);

            distanceFromLastPosition = Vector3.Distance(currentPosition, nextPosition);
        }
        AddRingAtPoint(trackRingsData, currentPosition, localForward, localUp, distanceFromLastPosition);
        CreateTrackSegment(trackRingsData);
    }

    private void AddRingAtPoint(TrackRingsData trackRingsData, Vector3 currentPosition, Vector3 localForward, Vector3 localUp, float distanceFromLastPosition)
    {
        LocalPointData pointData = new LocalPointData(currentPosition, localForward, localUp);
        trackRingsData.GenerateRingAtPoint(pointData, distanceFromLastPosition);
    }
    private float GetProgressionAlongLoop(int ringIdx)
    {
        return (float)ringIdx / RINGS_PER_TRACK * _loopPercentage;
    }

    private float GetTheta(float progression)
    {
        return progression * 2f * Mathf.PI;
    }

    private Vector3 GetLoopPosition(float lateralOffset, float progression, float theta)
    {
        float x = lateralOffset * progression;
        float y = _radius * (1f - Mathf.Cos(theta));
        float z = _radius * Mathf.Sin(theta);

        return new Vector3(x, y, z);
    }

    private Vector3 GetLocalForward(float theta)
    {
        return new Vector3(0f, Mathf.Sin(theta), Mathf.Cos(theta));
    }

    private Vector3 GetLoopCenterAtX(float x)
    {
        return new Vector3(x, _radius, 0f);
    }

    private Vector3 GetRollOffset()
    {
        return new Vector3(_embankment, 0f, 0f);
    }

    private Vector3 GetLocalUp(Vector3 currentPosition, Vector3 loopCenter, Vector3 rollOffset, float theta)
    {
        return (loopCenter - currentPosition).normalized + (Mathf.Sin(theta) * rollOffset);
    }
}

