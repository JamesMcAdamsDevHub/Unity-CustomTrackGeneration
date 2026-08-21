using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackIntersection
{
    private string _namePrefix;
    private Material _deckMaterial;
    private Material _railMaterial;
    private Material _baseMaterial;
    private MeshData _deckMeshData;
    private MeshData _railMeshData;
    private MeshData _baseMeshData;

    public TrackIntersection(string namePrefix, Material deckMaterial, Material railMaterial, Material baseMaterial,
        MeshData deckMeshData, MeshData railMeshData, MeshData baseMeshData)
    {
        _namePrefix = namePrefix;
        _deckMaterial = deckMaterial;
        _railMaterial = railMaterial;
        _baseMaterial = baseMaterial;
        _deckMeshData = deckMeshData;
        _railMeshData = railMeshData;
        _baseMeshData = baseMeshData;
    }
    public GameObject Generate()
    {
        GameObject trackIntersectionObject;
        trackIntersectionObject = new GameObject("Track_Segment");
        trackIntersectionObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        trackIntersectionObject.transform.localScale = Vector3.one;

        GameObject deckObject;
        deckObject = new GameObject(_namePrefix + "_Deck");
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(deckObject, "Create Deck Component");
#endif
        deckObject.transform.SetParent(trackIntersectionObject.transform, false);
        MeshFilter deckMeshFilter = deckObject.AddComponent<MeshFilter>();
        MeshRenderer deckMeshRenderer = deckObject.AddComponent<MeshRenderer>();
        MeshCollider deckMeshCollider = deckObject.AddComponent<MeshCollider>();
        deckMeshRenderer.sharedMaterial = _deckMaterial;

        Mesh deckMesh = new Mesh();
        deckMesh.name = _namePrefix + "_Deck_Mesh";
        deckMeshFilter.sharedMesh = deckMesh;
        deckMesh.SetVertices(_deckMeshData.vertices);
        deckMesh.SetTriangles(_deckMeshData.triangles, 0);
        deckMesh.SetUVs(0, _deckMeshData.uvs);
        deckMesh.RecalculateNormals();
        deckMesh.RecalculateBounds();
        deckMeshCollider.sharedMesh = deckMesh;

        GameObject railObject;
        railObject = new GameObject(_namePrefix + "_Rail");
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(railObject, "Create Rail Component");
#endif
        railObject.transform.SetParent(trackIntersectionObject.transform, false);
        MeshFilter railMeshFilter = railObject.AddComponent<MeshFilter>();
        MeshRenderer railMeshRenderer = railObject.AddComponent<MeshRenderer>();
        MeshCollider railMeshCollider = railObject.AddComponent<MeshCollider>();
        railMeshRenderer.sharedMaterial = _railMaterial;

        Mesh railMesh = new Mesh();
        railMesh.name = _namePrefix + "_Rail_Mesh";
        railMeshFilter.sharedMesh = railMesh;
        railMesh.SetVertices(_railMeshData.vertices);
        railMesh.SetTriangles(_railMeshData.triangles, 0);
        railMesh.SetUVs(0, _railMeshData.uvs);
        railMesh.RecalculateNormals();
        railMesh.RecalculateBounds();
        railMeshCollider.sharedMesh = railMesh;

        GameObject baseObject;
        baseObject = new GameObject(_namePrefix + "_Base");
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(baseObject, "Create Base Component");
#endif
        baseObject.transform.SetParent(trackIntersectionObject.transform, false);
        MeshFilter baseMeshFilter = baseObject.AddComponent<MeshFilter>();
        MeshRenderer baseMeshRenderer = baseObject.AddComponent<MeshRenderer>();
        MeshCollider baseMeshCollider = baseObject.AddComponent<MeshCollider>();
        baseMeshRenderer.sharedMaterial = _baseMaterial;

        Mesh baseMesh = new Mesh();
        baseMesh.name = _namePrefix + "_Base_Mesh";
        baseMeshFilter.sharedMesh = baseMesh;
        baseMesh.SetVertices(_baseMeshData.vertices);
        baseMesh.SetTriangles(_baseMeshData.triangles, 0);
        baseMesh.SetUVs(0, _baseMeshData.uvs);
        baseMesh.RecalculateNormals();
        baseMesh.RecalculateBounds();
        baseMeshCollider.sharedMesh = baseMesh;

        return trackIntersectionObject;
    }
}
