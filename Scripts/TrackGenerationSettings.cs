using UnityEngine;

[System.Serializable]
public class TrackGenerationSettings
{
    [Header("Default Track Configuration")]
    public bool useConfig;
    public TrackGenerationDefaultConfig trackConfig;

    [Header("Track Constraints")]
    [Range(50f, 300f)] public float trackWidth = 100f;
    [Range(0.1f, 10f)] public float trackHeight = 1f;
    [Range(1f, 20f)] public float railWidth = 2f;
    [Range(0f, 20f)] public float railRidgeHeight = 1f;
    [Range(0f, 1f), Tooltip("Ridge Position [0,1]. 0 = Vertical inner edge, 1 = Vertical outer edge")]
    public float railRidgePosition = 0.5f;
    [Range(0.5f, 200f), Tooltip("Smaller Distance = Smoother Track")]
    public float distanceBetweenRings = 5f;

    [Header("Track Materials")]
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
        railRidgeHeight = trackConfig.RailRidgeHeight;
        railRidgePosition = trackConfig.RailRidgePosition;
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
        railWidth = Mathf.Clamp(railWidth, 1f, 20f);
        railRidgeHeight = Mathf.Clamp(railRidgeHeight, 0f, 20f);
        railRidgePosition = Mathf.Clamp(railRidgePosition, 0f, 1f);
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
        data.RailRidgePosition = railRidgePosition;
        data.DistanceBetweenRings = distanceBetweenRings;
        data.DeckMaterialTileSize = deckMaterialTileSize;
        data.RailMaterialTileSize = railMaterialTileSize;
        data.BaseMaterialTileSize = baseMaterialTileSize;
    }
}