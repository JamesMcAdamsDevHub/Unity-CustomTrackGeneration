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
        railMeshData.vertices.Clear();
        railMeshData.triangles.Clear();
        railMeshData.uvs.Clear();
        baseMeshData.vertices.Clear();
        baseMeshData.triangles.Clear();
        baseMeshData.uvs.Clear();

        // Dimension Variables
        Vector3 forward = Vector3.forward;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.right;

        Vector3 trackWidthFromCenter = right * (_trackConstraintsData.TrackWidth / 2);
        Vector3 trackHeight = up * _trackConstraintsData.TrackHeight;
        Vector3 trackDepth = -forward * _trackConstraintsData.RailWidth;
        Vector3 railWidthFromCenter = trackWidthFromCenter - (right * _trackConstraintsData.RailWidth);
        Vector3 railRidgeTotalHeight = trackHeight + (up * _trackConstraintsData.RailRidgeHeight);
        Vector3 railRidgeWidthFromCenter = railWidthFromCenter + (right * _trackConstraintsData.RailRidgePosition * _trackConstraintsData.RailWidth);
        Vector3 railRidgeDepth = -forward * (_trackConstraintsData.RailRidgePosition * _trackConstraintsData.RailWidth);

        // BASE //

        // Left side
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackDepth);
        baseMeshData.vertices.Add(-trackWidthFromCenter);
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackHeight + trackDepth);
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackHeight);

        AddRectangularSetOfTriangles(baseMeshData);
        AddRectangularUVs(baseMeshData, _trackConstraintsData.RailWidth);

        // Right side
        baseMeshData.vertices.Add(trackWidthFromCenter);
        baseMeshData.vertices.Add(trackWidthFromCenter + trackDepth);
        baseMeshData.vertices.Add(trackWidthFromCenter + trackHeight);
        baseMeshData.vertices.Add(trackWidthFromCenter + trackHeight + trackDepth);

        AddRectangularSetOfTriangles(baseMeshData);
        AddRectangularUVs(baseMeshData, _trackConstraintsData.RailWidth);

        // Back
        baseMeshData.vertices.Add(trackWidthFromCenter + trackDepth);
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackDepth);
        baseMeshData.vertices.Add(trackWidthFromCenter + trackHeight + trackDepth);
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackHeight + trackDepth);

        AddRectangularSetOfTriangles(baseMeshData);
        AddRectangularUVs(baseMeshData, _trackConstraintsData.TrackWidth);

        // Bottom
        baseMeshData.vertices.Add(-trackWidthFromCenter + trackDepth);
        baseMeshData.vertices.Add(trackWidthFromCenter + trackDepth);
        baseMeshData.vertices.Add(-trackWidthFromCenter);
        baseMeshData.vertices.Add(trackWidthFromCenter);
        
        
        

        AddRectangularSetOfTriangles(baseMeshData);
        AddRectangularUVs(baseMeshData, _trackConstraintsData.TrackWidth);

        // RAIL //

        // Left rectangle to ridge
        railMeshData.vertices.Add(-trackWidthFromCenter + trackHeight + railRidgeDepth);
        railMeshData.vertices.Add(-trackWidthFromCenter + trackHeight);
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + railRidgeTotalHeight);

        AddRectangularSetOfTriangles(railMeshData);
        AddRectangularUVs(railMeshData, _trackConstraintsData.RailWidth);

        // Right rectangle to ridge
        railMeshData.vertices.Add(trackWidthFromCenter + trackHeight);
        railMeshData.vertices.Add(trackWidthFromCenter + trackHeight + railRidgeDepth);
        railMeshData.vertices.Add(railRidgeWidthFromCenter + railRidgeTotalHeight);
        railMeshData.vertices.Add(railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);

        AddRectangularSetOfTriangles(railMeshData);
        AddRectangularUVs(railMeshData, _trackConstraintsData.RailWidth);

        // Back center rectangle
        railMeshData.vertices.Add(railWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(-railWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(railWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(-railWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);

        AddRectangularSetOfTriangles(railMeshData);
        AddRectangularUVs(railMeshData, (_trackConstraintsData.TrackWidth - (_trackConstraintsData.RailRidgePosition * _trackConstraintsData.RailWidth * 2)));

        // Back left rectangle
        railMeshData.vertices.Add(-railWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(-railWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);

        AddRectangularSetOfTriangles(railMeshData);
        AddRectangularUVs(railMeshData, _trackConstraintsData.RailWidth);

        // Back right rectangle
        railMeshData.vertices.Add(railRidgeWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(railWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(railWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        

        AddRectangularSetOfTriangles(railMeshData);
        AddRectangularUVs(railMeshData, _trackConstraintsData.RailWidth);

        // Front rectangle ridge to track
        railMeshData.vertices.Add(railWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(-railWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(railWidthFromCenter + trackHeight);
        railMeshData.vertices.Add(-railWidthFromCenter + trackHeight);

        AddRectangularSetOfTriangles(railMeshData);
        AddRectangularUVs(railMeshData, (_trackConstraintsData.TrackWidth - (_trackConstraintsData.RailRidgePosition * _trackConstraintsData.RailWidth * 2)));

        // Left triangle to ridge 1
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + railRidgeTotalHeight);
        railMeshData.vertices.Add(-railWidthFromCenter + trackHeight);

        AddTriangularTriangle(railMeshData);
        AddTriangularUVs(railMeshData);

        // Left triangle to ridge 2
        railMeshData.vertices.Add(-railWidthFromCenter + trackHeight);
        railMeshData.vertices.Add(-railWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);

        AddTriangularTriangle(railMeshData);
        AddTriangularUVs(railMeshData);

        // Right triangle to ridge 1
        railMeshData.vertices.Add(railWidthFromCenter + trackHeight);
        railMeshData.vertices.Add(railRidgeWidthFromCenter + railRidgeTotalHeight);
        railMeshData.vertices.Add(railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);

        AddTriangularTriangle(railMeshData);
        AddTriangularUVs(railMeshData);

        // Right triangle to ridge 2
        railMeshData.vertices.Add(railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(railWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(railWidthFromCenter + trackHeight);

        AddTriangularTriangle(railMeshData);
        AddTriangularUVs(railMeshData);

        // Back-left triangle to ridge 1
        railMeshData.vertices.Add(-trackWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(-trackWidthFromCenter + trackHeight + railRidgeDepth);
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);

        AddTriangularTriangle(railMeshData);
        AddTriangularUVs(railMeshData);

        // Back-left triangle to ridge 2
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(-railRidgeWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(-trackWidthFromCenter + trackHeight + trackDepth);

        AddTriangularTriangle(railMeshData);
        AddTriangularUVs(railMeshData);

        // Back-right triangle to ridge 1
        railMeshData.vertices.Add(railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);
        railMeshData.vertices.Add(trackWidthFromCenter + trackHeight + railRidgeDepth);
        railMeshData.vertices.Add(trackWidthFromCenter + trackHeight + trackDepth);


        AddTriangularTriangle(railMeshData);
        AddTriangularUVs(railMeshData);

        // Back-right triangle to ridge 2
        railMeshData.vertices.Add(trackWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(railRidgeWidthFromCenter + trackHeight + trackDepth);
        railMeshData.vertices.Add(railRidgeWidthFromCenter + railRidgeTotalHeight + railRidgeDepth);

        AddTriangularTriangle(railMeshData);
        AddTriangularUVs(railMeshData);
    }

    void AddRectangularUVs(MeshData meshData, float width)
    {
        int U1 = (int)((width / _trackConstraintsData.RailWidth));
        meshData.uvs.Add(new Vector2(0, 0)); // Left-Bottom
        meshData.uvs.Add(new Vector2(U1, 0)); // Right-Bottom
        meshData.uvs.Add(new Vector2(0, 1)); // Left-Top
        meshData.uvs.Add(new Vector2(U1, 1)); // Right-Top
    }

    void AddRectangularSetOfTriangles(MeshData meshData)
    {
        int startIdx = meshData.vertices.Count - 4;
        meshData.triangles.Add(startIdx); meshData.triangles.Add(startIdx + 1); meshData.triangles.Add(startIdx + 2);
        meshData.triangles.Add(startIdx + 2); meshData.triangles.Add(startIdx + 1); meshData.triangles.Add(startIdx + 3);
    }

    void AddTriangularUVs(MeshData meshData)
    {
        meshData.uvs.Add(new Vector2(1, 0)); // Left-Bottom
        meshData.uvs.Add(new Vector2(0, 0)); // Right-Bottom
        meshData.uvs.Add(new Vector2(0, 1)); // Left-Top
    }

    void AddTriangularTriangle(MeshData meshData)
    {
        int startIdx = meshData.vertices.Count - 3;
        meshData.triangles.Add(startIdx); meshData.triangles.Add(startIdx + 1); meshData.triangles.Add(startIdx + 2);
    }
}