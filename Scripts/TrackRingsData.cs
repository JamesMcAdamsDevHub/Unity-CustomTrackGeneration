using UnityEngine;

public class TrackRingsData
{
    private TrackConstraintsData _trackConstraintsData;
    public MeshData deckMeshData;
    public MeshData railMeshData;
    public MeshData baseMeshData;

    public TrackRingsData(TrackConstraintsData trackConstraintsData)
    {
        this._trackConstraintsData = trackConstraintsData;
    }

    public void GenerateEndcapAtPoint(Vector3 worldPosition, Vector3 forward, Vector3 up)
    {
        // Generate and store all MeshData for a ring with trackConstraints and Transform defined by function arguments
    }
}
