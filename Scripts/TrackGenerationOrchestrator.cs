using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class TrackGenerationOrchestrator : MonoBehaviour
{
    [SerializeField] protected TrackGenerationSettings _settings = new TrackGenerationSettings();

    protected TrackConstraintsData _trackConstraintsData = new TrackConstraintsData();

    protected const int MAX_VERTS_PER_TRACK = 6000;
    protected const int VERTS_PER_RING = 20;
    protected const int RINGS_PER_TRACK = MAX_VERTS_PER_TRACK / VERTS_PER_RING;

    protected abstract string ROOT_NAME { get; }

    protected abstract void GenerateNewTrack();

    protected virtual void OnValidate()
    {
        if (_settings != null)
        {
            _settings.UpdateConstraints(this);
        }
    }

    public void GenerateTrack()
    {
        if (_settings == null) return;

        _settings.CopyTo(_trackConstraintsData);
        
        DestroyPreviousTrack();

        GenerateNewTrack();
    }

    public void RefreshFromConfig()
    {
        if (_settings != null)
        {
            _settings.UpdateConstraints(this);
        }
    }

    protected Transform GetOrCreateRoot()
    {
        Transform existing = transform.Find(ROOT_NAME);
        if (existing != null)
            return existing;

        GameObject root = new GameObject(ROOT_NAME);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(root, "Create Track Root");
        Undo.SetTransformParent(root.transform, transform, "Attach root to parent");
#endif

        return root.transform;
    }

    protected void GenerateSpecifiedEndcap(LocalPointData point)
    {
#if UNITY_EDITOR
        Quaternion localRotation = Quaternion.LookRotation(point.localForward, point.localUp);
        TrackEndcapData endcapData = new TrackEndcapData(_trackConstraintsData);
        endcapData.GenerateEndcapData();
        TrackEndcap endcap = new TrackEndcap(_settings.railMaterial, _settings.baseMaterial, endcapData.railMeshData, endcapData.baseMeshData);
        GameObject endcapGO = endcap.Generate(point.localPosition, localRotation);
        Undo.RegisterCreatedObjectUndo(endcapGO, "Create Endcap");
        Transform root = GetOrCreateRoot();
        Undo.SetTransformParent(endcapGO.transform, root, "Attach endcap to root");
#endif
    }

    private void DestroyPreviousTrack()
    {
        Transform root = transform.Find(ROOT_NAME);

        if (root == null) return;

#if UNITY_EDITOR
        Undo.DestroyObjectImmediate(root.gameObject);
#endif
    }

    protected void CreateTrackSegment(MeshData deckMeshData, MeshData railMeshData, MeshData baseMeshData)
    {
#if UNITY_EDITOR
        TrackSegment trackSegment = new TrackSegment(_settings.deckMaterial, _settings.railMaterial, _settings.baseMaterial,
                deckMeshData, railMeshData, baseMeshData);
        GameObject trackSegmentGO = trackSegment.Generate();
        Undo.RegisterCreatedObjectUndo(trackSegmentGO, "Create Track Segment");
        Transform root = GetOrCreateRoot();
        Undo.SetTransformParent(trackSegmentGO.transform, root, "Attach trackSegment to root");
#endif
    }
}