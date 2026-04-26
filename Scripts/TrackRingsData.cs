using System.Drawing;
using UnityEngine;

public class TrackRingsData
{
    private TrackConstraintsData _trackConstraintsData;
    public MeshData deckMeshData = new MeshData();
    public MeshData railMeshData = new MeshData();
    public MeshData baseMeshData = new MeshData();

    private float _previousDeckV = 0f;
    private float _currentDeckV = 0f;
    private float _previousRailU = 0f;
    private float _currentRailU = 0f;
    private float _previousBaseU = 0f;
    private float _currentBaseU = 0f;

    private float _distanceAlongTrack = 0f;

    public TrackRingsData(TrackConstraintsData trackConstraintsData)
    {
        _trackConstraintsData = trackConstraintsData;
    }

    public void GenerateRingAtPoint(LocalPointData point, float distanceFromLastRing)
    {
        TrackRingVectorData vectorData = new TrackRingVectorData(_trackConstraintsData, point.localForward, point.localUp);

        _distanceAlongTrack += distanceFromLastRing;
        
        updateCurrentU();

        AddDeckVerts(vectorData, point.localPosition);
        AddDeckTriangles();
        AddDeckUVs();

        AddRailVerts(vectorData, point.localPosition);
        AddRailTriangles();
        AddRailUVs();

        AddBaseVerts(vectorData, point.localPosition);
        AddBaseTriangles();
        AddBaseUVs();

        updatePreviousU();
    }

    private void updateCurrentU()
    {
        _currentDeckV = _trackConstraintsData.DeckMaterialTileSize * _distanceAlongTrack / 40f;
        _currentRailU = _trackConstraintsData.RailMaterialTileSize * _distanceAlongTrack / 20f;
        _currentBaseU = _trackConstraintsData.BaseMaterialTileSize * _distanceAlongTrack / 20f;
    }

    private void updatePreviousU()
    {
        _previousDeckV = _currentDeckV;
        _previousRailU = _currentRailU;
        _previousBaseU = _currentBaseU;
    }

    private void AddDeckVerts(TrackRingVectorData vectorData, Vector3 localPosition)
    {
        int idx = deckMeshData.vertices.Count;
        if (idx > 2)
        {
            for (int i = 2; i > 0; i--)
            {
                deckMeshData.vertices.Add(deckMeshData.vertices[idx - i]);
            }
        }

        // Deck
        deckMeshData.vertices.Add(localPosition + vectorData.TrackWidthFromCenter + vectorData.TrackHeight);
        deckMeshData.vertices.Add(localPosition - vectorData.TrackWidthFromCenter + vectorData.TrackHeight);
    }

    private void AddDeckTriangles()
    {
        if (deckMeshData.vertices.Count == 2) return;

        int startIdx = deckMeshData.vertices.Count - 4;
        int nextIdx = 2;

        // Left outer face
        deckMeshData.triangles.Add(startIdx); deckMeshData.triangles.Add(startIdx + nextIdx); deckMeshData.triangles.Add(startIdx + 1);
        deckMeshData.triangles.Add(startIdx + 1); deckMeshData.triangles.Add(startIdx + nextIdx); deckMeshData.triangles.Add(startIdx + 1 + nextIdx);
    }

    private void AddDeckUVs()
    {
        if (deckMeshData.vertices.Count == 2)
            return;

        // Previous duplicated vertices
        deckMeshData.uvs.Add(new Vector2(0f, _previousDeckV));
        deckMeshData.uvs.Add(new Vector2(1f, _previousDeckV));
        

        // Current ring vertices
        deckMeshData.uvs.Add(new Vector2(0f, _currentDeckV));
        deckMeshData.uvs.Add(new Vector2(1f, _currentDeckV));

    }

    private void AddRailVerts(TrackRingVectorData vectorData, Vector3 localPosition)
    {
        int idx = railMeshData.vertices.Count;
        int vertsPerRing = _trackConstraintsData.useSplitRidge ? 12 : 8;

        if (idx > vertsPerRing)
        {
            for (int i = vertsPerRing; i > 0; i--)
            {
                railMeshData.vertices.Add(railMeshData.vertices[idx - i]);
            }
        }

        // Left outer face
        railMeshData.vertices.Add(localPosition - vectorData.RailOuterRidgeWidthFromCenter + vectorData.RailRidgeTotalHeight);
        railMeshData.vertices.Add(localPosition - vectorData.TrackWidthFromCenter + vectorData.TrackHeight);

        // Left inner face
        railMeshData.vertices.Add(localPosition - vectorData.RailWidthFromCenter + vectorData.TrackHeight);
        railMeshData.vertices.Add(localPosition - vectorData.RailInnerRidgeWidthFromCenter + vectorData.RailRidgeTotalHeight);

        // Right outer face
        railMeshData.vertices.Add(localPosition + vectorData.TrackWidthFromCenter + vectorData.TrackHeight);
        railMeshData.vertices.Add(localPosition + vectorData.RailOuterRidgeWidthFromCenter + vectorData.RailRidgeTotalHeight);
        

        // Right inner face
        railMeshData.vertices.Add(localPosition + vectorData.RailInnerRidgeWidthFromCenter + vectorData.RailRidgeTotalHeight);
        railMeshData.vertices.Add(localPosition + vectorData.RailWidthFromCenter + vectorData.TrackHeight);

        // If useSplitRidge = true, add verts to connect ridge points
        if (_trackConstraintsData.useSplitRidge)
        {
            // Left top face
            railMeshData.vertices.Add(localPosition - vectorData.RailInnerRidgeWidthFromCenter + vectorData.RailRidgeTotalHeight);
            railMeshData.vertices.Add(localPosition - vectorData.RailOuterRidgeWidthFromCenter + vectorData.RailRidgeTotalHeight);

            // Right top face
            railMeshData.vertices.Add(localPosition + vectorData.RailInnerRidgeWidthFromCenter + vectorData.RailRidgeTotalHeight);
            railMeshData.vertices.Add(localPosition + vectorData.RailOuterRidgeWidthFromCenter + vectorData.RailRidgeTotalHeight);
        }
    }

    private void AddRailTriangles()
    {
        if (railMeshData.vertices.Count == 8 || (_trackConstraintsData.useSplitRidge && railMeshData.vertices.Count == 12))
            return;

        int vertsPerRing = _trackConstraintsData.useSplitRidge ? 12 : 8;

        int startIdx = railMeshData.vertices.Count - (vertsPerRing * 2);
        int nextIdx = vertsPerRing;

        // Left outer face
        railMeshData.triangles.Add(startIdx); railMeshData.triangles.Add(startIdx + nextIdx); railMeshData.triangles.Add(startIdx + 1);
        railMeshData.triangles.Add(startIdx + 1); railMeshData.triangles.Add(startIdx + nextIdx); railMeshData.triangles.Add(startIdx + 1 + nextIdx);

        // Left inner face
        railMeshData.triangles.Add(startIdx + 3); railMeshData.triangles.Add(startIdx + 2 + nextIdx); railMeshData.triangles.Add(startIdx + 3 + nextIdx);
        railMeshData.triangles.Add(startIdx + 2); railMeshData.triangles.Add(startIdx + 2 + nextIdx); railMeshData.triangles.Add(startIdx + 3);

        // Right inner face
        railMeshData.triangles.Add(startIdx + 6); railMeshData.triangles.Add(startIdx + 6 + nextIdx); railMeshData.triangles.Add(startIdx + 7);
        railMeshData.triangles.Add(startIdx + 7); railMeshData.triangles.Add(startIdx + 6 + nextIdx); railMeshData.triangles.Add(startIdx + 7 + nextIdx);

        // Right outer face
        railMeshData.triangles.Add(startIdx + 4); railMeshData.triangles.Add(startIdx + 4 + nextIdx); railMeshData.triangles.Add(startIdx + 5);
        railMeshData.triangles.Add(startIdx + 5); railMeshData.triangles.Add(startIdx + 4 + nextIdx); railMeshData.triangles.Add(startIdx + 5 + nextIdx);

        // If useSplitRidge = true, add triangles to connect ridge points
        if (_trackConstraintsData.useSplitRidge)
        {
            // Top ridge face
            railMeshData.triangles.Add(startIdx + 9 + nextIdx); railMeshData.triangles.Add(startIdx + 9); railMeshData.triangles.Add(startIdx + 8 + nextIdx);
            railMeshData.triangles.Add(startIdx + 8); railMeshData.triangles.Add(startIdx + 8 + nextIdx); railMeshData.triangles.Add(startIdx + 9);

            // Top ridge face
            railMeshData.triangles.Add(startIdx + 10 + nextIdx); railMeshData.triangles.Add(startIdx + 10); railMeshData.triangles.Add(startIdx + 11 + nextIdx);
            railMeshData.triangles.Add(startIdx + 11); railMeshData.triangles.Add(startIdx + 11 + nextIdx); railMeshData.triangles.Add(startIdx + 10);
        }
    }

    private void AddRailUVs()
    {
        if (railMeshData.vertices.Count == 8 || (_trackConstraintsData.useSplitRidge && railMeshData.vertices.Count == 12))
            return;

        // Previous Vertices //

        // Left outer face
        railMeshData.uvs.Add(new Vector2(_previousRailU, 1f)); 
        railMeshData.uvs.Add(new Vector2(_previousRailU, 0f)); 

        // Left inner face
        railMeshData.uvs.Add(new Vector2(_previousRailU, 0f)); 
        railMeshData.uvs.Add(new Vector2(_previousRailU, 1f)); 

        // Right outer face
        railMeshData.uvs.Add(new Vector2(_previousRailU, 0f)); 
        railMeshData.uvs.Add(new Vector2(_previousRailU, 1f)); 

        // Right inner face
        railMeshData.uvs.Add(new Vector2(_previousRailU, 1f)); 
        railMeshData.uvs.Add(new Vector2(_previousRailU, 0f));

        // If useSplitRidge = true, add UVs to connect ridge points
        if (_trackConstraintsData.useSplitRidge)
        {
            railMeshData.uvs.Add(new Vector2(_previousRailU, 1f));
            railMeshData.uvs.Add(new Vector2(_previousRailU, 0f));

            railMeshData.uvs.Add(new Vector2(_previousRailU, 1f));
            railMeshData.uvs.Add(new Vector2(_previousRailU, 0f));
        }

        // Current Vertices //

        // Left outer face
        railMeshData.uvs.Add(new Vector2(_currentRailU, 1f)); 
        railMeshData.uvs.Add(new Vector2(_currentRailU, 0f)); 

        // Left inner face
        railMeshData.uvs.Add(new Vector2(_currentRailU, 0f)); 
        railMeshData.uvs.Add(new Vector2(_currentRailU, 1f)); 

        // Right outer face
        railMeshData.uvs.Add(new Vector2(_currentRailU, 0f));
        railMeshData.uvs.Add(new Vector2(_currentRailU, 1f)); 

        // Right inner face
        railMeshData.uvs.Add(new Vector2(_currentRailU, 1f));
        railMeshData.uvs.Add(new Vector2(_currentRailU, 0f));

        // If useSplitRidge = true, add UVs to connect ridge points
        if (_trackConstraintsData.useSplitRidge)
        {
            railMeshData.uvs.Add(new Vector2(_currentRailU, 1f));
            railMeshData.uvs.Add(new Vector2(_currentRailU, 0f));

            railMeshData.uvs.Add(new Vector2(_currentRailU, 1f));
            railMeshData.uvs.Add(new Vector2(_currentRailU, 0f));
        }
    }

    private void AddBaseVerts (TrackRingVectorData vectorData, Vector3 localPosition)
    {   
        int idx = baseMeshData.vertices.Count;
        if (idx > 6)
        {
            for (int i = 6; i > 0; i--)
            {
                baseMeshData.vertices.Add(baseMeshData.vertices[idx - i]);
            }
        }

        // Left side
        baseMeshData.vertices.Add(localPosition - vectorData.TrackWidthFromCenter + vectorData.TrackHeight);
        baseMeshData.vertices.Add(localPosition - vectorData.TrackWidthFromCenter);

        // Right side
        baseMeshData.vertices.Add(localPosition + vectorData.TrackWidthFromCenter);
        baseMeshData.vertices.Add(localPosition + vectorData.TrackWidthFromCenter + vectorData.TrackHeight);

        // Bottom
        baseMeshData.vertices.Add(localPosition - vectorData.TrackWidthFromCenter);
        baseMeshData.vertices.Add(localPosition + vectorData.TrackWidthFromCenter);
    }

    private void AddBaseTriangles()
    {
        if (baseMeshData.vertices.Count == 6) return;

        int startIdx = baseMeshData.vertices.Count - 12;
        int nextIdx = 6;

        // Left side
        baseMeshData.triangles.Add(startIdx); baseMeshData.triangles.Add(startIdx + nextIdx); baseMeshData.triangles.Add(startIdx + 1);
        baseMeshData.triangles.Add(startIdx + 1); baseMeshData.triangles.Add(startIdx + nextIdx); baseMeshData.triangles.Add(startIdx + 1 + nextIdx);
        
        // Right side
        baseMeshData.triangles.Add(startIdx + 3); baseMeshData.triangles.Add(startIdx + 2 + nextIdx); baseMeshData.triangles.Add(startIdx + 3 + nextIdx);
        baseMeshData.triangles.Add(startIdx + 2); baseMeshData.triangles.Add(startIdx + 2 + nextIdx); baseMeshData.triangles.Add(startIdx + 3);

        // Bottom
        baseMeshData.triangles.Add(startIdx + 4); baseMeshData.triangles.Add(startIdx + 4 + nextIdx); baseMeshData.triangles.Add(startIdx + 5);
        baseMeshData.triangles.Add(startIdx + 5); baseMeshData.triangles.Add(startIdx + 4 + nextIdx); baseMeshData.triangles.Add(startIdx + 5 + nextIdx);
    }
    private void AddBaseUVs()
    {
        if (baseMeshData.vertices.Count == 6)
            return;

        for (int i = 0; i < 3; i++)
        {
            // Previous duplicated vertices
            baseMeshData.uvs.Add(new Vector2(_previousBaseU, 0f));
            baseMeshData.uvs.Add(new Vector2(_previousBaseU, 1f));

            // Current ring vertices
            baseMeshData.uvs.Add(new Vector2(_currentBaseU, 0f));
            baseMeshData.uvs.Add(new Vector2(_currentBaseU, 1f));
        }
    }
}
