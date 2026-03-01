using UnityEngine;

public class TrackEndcap
{
    private Material _railMaterial;
    private Material _baseMaterial;
    private MeshData _railMeshData;
    private MeshData _baseMeshData;

    public TrackEndcap(Material railMaterial, Material baseMaterial, 
        MeshData railMeshData, MeshData baseMeshData)
    {
        this._railMaterial = railMaterial;
        this._baseMaterial = baseMaterial;
        this._railMeshData = railMeshData;
        this._baseMeshData = baseMeshData;
    }

    public GameObject Generate()
    {
        // TODO: Create and return new Endcap GameObject using local fields
        return new GameObject(); // PLACEHOLDER
    }
}
