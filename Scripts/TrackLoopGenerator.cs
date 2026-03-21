using UnityEngine;

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

    protected override string ROOT_NAME => "Loop_Root";

    protected override void GenerateNewTrack()
    {
        TrackRingsData trackRingsData = new TrackRingsData(_trackConstraintsData);
        float yTotalOffset = (_loopGap == 0f) ? 0f : _settings.trackWidth + _loopGap;

        Vector3 localForward = Vector3.forward;
        Vector3 localUp = Vector3.up;
        Vector3 currPos = Vector3.zero;
        float distanceToNextPos = 0f;
        for (int i = 1; i <= RINGS_PER_TRACK + 1; i++)
        {
            LocalPointData newPoint = new LocalPointData(currPos, localForward, localUp);
            trackRingsData.GenerateRingAtPoint(newPoint, distanceToNextPos);

            if (i > RINGS_PER_TRACK) break;

            float progression = (float)i / RINGS_PER_TRACK * _loopPercentage;
            float theta = progression * 2f * Mathf.PI;
            float x = yTotalOffset * progression;
            float y = _radius * (1f - Mathf.Cos(theta));
            float z = _radius * Mathf.Sin(theta);

            currPos = new Vector3(x, y, z);

            float nextProgression = (float)(i + 1) / RINGS_PER_TRACK * _loopPercentage;
            float nextTheta = nextProgression * 2f * Mathf.PI;
            float nextX = yTotalOffset * nextProgression;
            float nextY = _radius * (1f - Mathf.Cos(nextTheta));
            float nextZ = _radius * Mathf.Sin(nextTheta);
            Vector3 nextPos = new Vector3(nextX, nextY, nextZ);

            localForward = new Vector3(0f, Mathf.Sin(theta), Mathf.Cos(theta));

            Vector3 loopCenter = new Vector3(x, _radius, 0);
            localUp = (loopCenter - currPos).normalized;

            distanceToNextPos = Vector3.Distance(currPos, nextPos);
        }

        CreateTrackSegment(trackRingsData.deckMeshData, trackRingsData.railMeshData, trackRingsData.baseMeshData);
    }
}

