using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackLoopGenerator : TrackGenerationOrchestrator
{
    [Header("Loop Dimensions")]

    [SerializeField, Range(50f, 300f),
        Tooltip("Radius of the generated loop.")] 
    private float _radius;

    [SerializeField, Range(0.2f, 1f), 
        Tooltip("Normalized percentage of a full loop to generate.")] 
    private float _loopPercentage;

    [SerializeField, Range(0f, 500f), 
        Tooltip("Separation between the rising and falling sides of the loop. A value of 0 creates a circular loop. " +
        "Values above 0 offset the loop to avoid self-overlap.")] 
    private float _loopGap;

    protected override string ROOT_NAME => "Loop_Root";

    protected override void GenerateNewTrack()
    {
        // TODO: Build Awesome Smooth Loop Defined By The User!
    }
}

