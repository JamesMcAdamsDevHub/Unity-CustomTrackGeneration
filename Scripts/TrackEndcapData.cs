using UnityEngine;

public class TrackEndcapData
{
    private TrackConstraintsData _trackConstraintsData;
    public MeshData railMeshData;
    public MeshData baseMeshData;

    public TrackEndcapData(TrackConstraintsData trackConstraintsData)
    {
        this._trackConstraintsData = trackConstraintsData;
    }

    public void GenerateEndcapAtPoint(Vector3 worldPosition, Vector3 forward, Vector3 up)
    {
        // Generate and store all MeshData for Endcap with local fields and function arguments
    }
}
