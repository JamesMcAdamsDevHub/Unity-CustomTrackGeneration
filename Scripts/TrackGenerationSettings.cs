using UnityEngine;

[System.Serializable]
public class TrackGenerationSettings
{
    [Header("Default Track Configuration")]
    public bool useConfig = true;
    public TrackGenerationDefaultConfig trackConfig;

    [Header("Track Dimensions")]
    [Range(50f, 300f)] public float trackWidth = 100f;
    [Range(0.1f, 10f)] public float trackHeight = 1f;

    [Header("Rail Dimensions")]
    [Range(1f, 25f)] public float railWidth = 2f;
    [Range(0f, 20f)] public float railRidgeHeight = 1f;
    public bool useSplitRidge = false;
    [Range(0f, 1f), Tooltip("Ridge Position [0,1]. 0 = Vertical inner edge, 1 = Vertical outer edge")]
    public float railRidgeOffset = 0.5f;

    [Header("Mesh Resolution")]
    [Range(0.5f, 200f), Tooltip("Distance between generated rings of vertices. Smaller values produce smoother geometry but increase vertex count.")]
    public float distanceBetweenRings = 5f;

    [Header("Materials")]
    public Material deckMaterial;
    [Range(0.01f, 5f)] public float deckMaterialTileSize;
    public Material railMaterial;
    [Range(0.01f, 5f)] public float railMaterialTileSize;
    public Material baseMaterial;
    [Range(0.01f, 5f)] public float baseMaterialTileSize;

    public void UpdateConstraints(Object context)
    {
        if (!useConfig)
        {
            EnforceConstraints();
            return;
        }

        if (trackConfig == null)
        {
            Debug.LogWarning("Track config is enabled, but no config is assigned.", context);
            return;
        }

        trackWidth = trackConfig.TrackWidth;
        trackHeight = trackConfig.TrackHeight;
        railWidth = trackConfig.RailWidth;
        useSplitRidge = trackConfig.useSplitRidge;
        railRidgeHeight = trackConfig.RailRidgeHeight;
        railRidgeOffset = trackConfig.RailRidgePosition;
        distanceBetweenRings = trackConfig.DistanceBetweenRings;

        if (trackConfig.DeckMaterial == null ||
            trackConfig.RailMaterial == null ||
            trackConfig.BaseMaterial == null)
        {
            Debug.LogWarning("Track config material(s) not assigned.", context);
            return;
        }

        deckMaterial = trackConfig.DeckMaterial;
        railMaterial = trackConfig.RailMaterial;
        baseMaterial = trackConfig.BaseMaterial;
        deckMaterialTileSize = trackConfig.DeckMaterialTileSize;
        railMaterialTileSize = trackConfig.RailMaterialTileSize;
        baseMaterialTileSize = trackConfig.BaseMaterialTileSize;
    }

    public void EnforceConstraints()
    {
        trackWidth = Mathf.Clamp(trackWidth, 50f, 300f);
        trackHeight = Mathf.Clamp(trackHeight, 0.1f, 10f);
        railWidth = Mathf.Clamp(railWidth, 1f, 25f);
        railRidgeHeight = Mathf.Clamp(railRidgeHeight, 0f, 20f);
        railRidgeOffset = Mathf.Clamp(railRidgeOffset, 0f, 1f);
        distanceBetweenRings = Mathf.Clamp(distanceBetweenRings, 0.5f, 200f);
        deckMaterialTileSize = Mathf.Clamp(deckMaterialTileSize, 0.01f, 5f);
        railMaterialTileSize = Mathf.Clamp(railMaterialTileSize, 0.01f, 5f);
        baseMaterialTileSize = Mathf.Clamp(baseMaterialTileSize, 0.01f, 5f);
    }
    public void CopyTo(TrackConstraintsData data)
    {
        data.TrackWidth = trackWidth;
        data.TrackHeight = trackHeight;
        data.RailWidth = railWidth;
        data.RailRidgeHeight = railRidgeHeight;
        data.useSplitRidge = useSplitRidge;
        data.RailRidgePosition = railRidgeOffset;
        data.DistanceBetweenRings = distanceBetweenRings;
        data.DeckMaterialTileSize = deckMaterialTileSize;
        data.RailMaterialTileSize = railMaterialTileSize;
        data.BaseMaterialTileSize = baseMaterialTileSize;
    }
}