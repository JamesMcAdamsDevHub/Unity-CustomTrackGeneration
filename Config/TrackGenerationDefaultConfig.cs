using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "TrackGenerationDefaultConfig", menuName = "Scriptable Objects/TrackGenerationDefaultConfig")]
public class TrackGenerationDefaultConfig : ScriptableObject
{
    [Header("Track Constraints")]
    [Range(10f, 300f)] public float TrackWidth;
    [Range(0.1f, 10f)] public float TrackHeight;
    [Range(1f, 20f)] public float RailWidth;
    [Range(0f, 20f)] public float RailRidgeHeight;
    [Range(0f, 1f), Tooltip("Ridge Position [0,1]. 0 = Vertical Inner Ridge, 1 = Vertical Outer Edge")] public float RailRidgePosition;
    [Range(0.5f, 200f), Tooltip("Smaller Distance = Smoother Track")] public float DistanceBetweenRings;

    [Header("Track Materials")]
    public Material DeckMaterial;
    [Range(0.01f, 5f)] public float DeckMaterialTileSize;
    public Material RailMaterial;
    [Range(0.01f, 5f)] public float RailMaterialTileSize;
    public Material BaseMaterial;
    [Range(0.01f, 5f)] public float BaseMaterialTileSize;

#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            foreach (var generator in Object.FindObjectsByType<TrackGenerationOrchestrator>(FindObjectsSortMode.None))
            {
                if (generator == null) continue;

                EditorUtility.SetDirty(generator);

                generator.RefreshFromConfig();
            }
        };
    }
#endif
}
