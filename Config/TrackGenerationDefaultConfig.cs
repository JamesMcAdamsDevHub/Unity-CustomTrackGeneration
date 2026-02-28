using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TrackGenerationDefaultConfig", menuName = "Scriptable Objects/TrackGenerationDefaultConfig")]
public class TrackGenerationDefaultConfig : ScriptableObject
{
    [Header("Track Constraints")]
    [Range(10f, 300f)] public float TrackWidth;
    [Range(1f, 10f)] public float TrackHeight;
    [Range(1f, 30f), Tooltip("Constrained: Max of 10% of trackWidth")] public float RailWidth;
    [Range(0f, 30f), Tooltip("Height of railRidge above track. Constrained: Max of trackHeight")] public float RailRidgeHeight;
    [Range(0f, 1f), Tooltip("Ridge Position [0,1]. 0 = Vertical Inner Ridge, 1 = Vertical Outer Edge")] public float RailRidgePosition;
    [Range(0.5f, 200f), Tooltip("Smaller Distance = Smoother Track")] public float DistanceBetweenRings;

    [Header("Track Materials")]
    public Material DeckMaterial;
    public Material RailMaterial;
    public Material BaseMaterial;

    void OnValidate()
    {
        // Enforce constraints
        TrackWidth = Mathf.Clamp(TrackWidth, 10f, 300f);
        TrackHeight = Mathf.Clamp(TrackHeight, 1f, 10f);

        RailWidth = Mathf.Max(1f, RailWidth);
        RailWidth = Mathf.Min(TrackWidth * 0.1f, RailWidth);
        RailRidgeHeight = Mathf.Max(0f, RailRidgeHeight);
        RailRidgeHeight = Mathf.Min(TrackHeight, RailRidgeHeight);

        RailRidgePosition = Mathf.Clamp(RailRidgePosition, 0f, 1f);
        DistanceBetweenRings = Mathf.Clamp(DistanceBetweenRings, 0.5f, 200f);
    }
}
