using UnityEngine;

public class TrackEndcapData
{
    private TrackConstraintsData _trackConstraintsData;
    public MeshData railMeshData = new MeshData();
    public MeshData baseMeshData = new MeshData();

    public TrackEndcapData(TrackConstraintsData trackConstraintsData)
    {
        this._trackConstraintsData = trackConstraintsData;
    }

    public void GenerateEndcapData()
    {
        // Dimension Variables
        Vector3 forward = Vector3.forward;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.right;

        Vector3 trackWidthFromCenter = right * (_trackConstraintsData.TrackWidth / 2);
        Vector3 trackHeight = up * _trackConstraintsData.TrackHeight;
        Vector3 trackDepth = -forward * _trackConstraintsData.RailWidth;
        Vector3 railWidthFromCenter = trackWidthFromCenter - (right * _trackConstraintsData.RailWidth);
        Vector3 railRidgeTotalHeight = trackHeight + (up * _trackConstraintsData.RailRidgeHeight);
        Vector3 railRidgeWidthFrontCenter = railWidthFromCenter + (right * _trackConstraintsData.RailRidgePosition * _trackConstraintsData.RailWidth);
        Vector3 railRidgeDepth = -forward * (_trackConstraintsData.RailRidgePosition * _trackConstraintsData.RailWidth);

        // BASE //

        // Left side
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackDepth);
        baseMeshData.vertices.Add(-trackWidthFromCenter);
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackHeight + trackDepth);
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackHeight);

        baseMeshData.triangles.Add(0); baseMeshData.triangles.Add(1); baseMeshData.triangles.Add(2);
        baseMeshData.triangles.Add(2); baseMeshData.triangles.Add(1); baseMeshData.triangles.Add(3);

        baseMeshData.uvs.Add(new Vector2(0, 0));
        baseMeshData.uvs.Add(new Vector2(1, 0));
        baseMeshData.uvs.Add(new Vector2(0, 1));
        baseMeshData.uvs.Add(new Vector2(1, 1));

        // Right side

        // Back

        // RAIL //

        // Left rectangle to ridge

        // Left triangle to ridge

        // Right rectangle to ridge

        // Right triangle to ridge

        // Back-left triangle to ridge

        // Back-right triangle to ridge

        // Back rectangle

        // Front-left triangle to ridge

        // Front-right triangle to ridge

        // Front ridge to track
    }
}
