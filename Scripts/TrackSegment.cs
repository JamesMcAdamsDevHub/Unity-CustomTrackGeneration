using UnityEngine;

public class TrackSegment
{
    private Material _deckMaterial;
    private Material _railMaterial;
    private Material _baseMaterial;
    private MeshData _deckMeshData;
    private MeshData _railMeshData;
    private MeshData _baseMeshData;

    public TrackSegment(Material deckMaterial, Material railMaterial, Material baseMaterial, 
        MeshData deckMeshData, MeshData railMeshData, MeshData baseMeshData)
    {
        this._deckMaterial = deckMaterial;
        this._railMaterial = railMaterial;
        this._baseMaterial = baseMaterial;
        this._deckMeshData = deckMeshData;
        this._railMeshData = railMeshData;
        this._baseMeshData = baseMeshData;
    }

    public GameObject Generate()
    {
        // TODO: Create and return new EndSegment GameObject using local fields
        return new GameObject(); // PLACEHOLDER
    }
}
