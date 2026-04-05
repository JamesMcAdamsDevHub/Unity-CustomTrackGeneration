using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackEndcap
{
    private Material _railMaterial;
    private Material _baseMaterial;
    private MeshData _railMeshData;
    private MeshData _baseMeshData;

    public TrackEndcap(Material railMaterial, Material baseMaterial, 
        MeshData railMeshData, MeshData baseMeshData)
    {
        _railMaterial = railMaterial;
        _baseMaterial = baseMaterial;
        _railMeshData = railMeshData;
        _baseMeshData = baseMeshData;
    }

    public GameObject Generate(Vector3 localPosition, Quaternion localRotation, string name)
    {
        GameObject endcapObject = new GameObject(name);
        endcapObject.transform.SetPositionAndRotation(localPosition, localRotation);
        endcapObject.transform.localScale = Vector3.one;

        GameObject railObject = new GameObject("Endcap_Rail");
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(railObject, "Create rail Component");
#endif
        railObject.transform.SetParent(endcapObject.transform, false);

        MeshFilter railMeshFilter = railObject.AddComponent<MeshFilter>();
        MeshRenderer railMeshRenderer = railObject.AddComponent<MeshRenderer>();
        MeshCollider railMeshCollider = railObject.AddComponent<MeshCollider>();
        railMeshRenderer.sharedMaterial = _railMaterial;

        Mesh railMesh = new Mesh { name = "Endcap_Rail_Mesh" };
        railMesh.SetVertices(_railMeshData.vertices);
        railMesh.SetTriangles(_railMeshData.triangles, 0);
        railMesh.SetUVs(0, _railMeshData.uvs);
        railMesh.RecalculateNormals();
        railMesh.RecalculateBounds();
        railMeshFilter.sharedMesh = railMesh;

        if (_railMeshData.vertices != null && _railMeshData.triangles != null &&
            _railMeshData.vertices.Count >= 3 && _railMeshData.triangles.Count >= 3)
        {
            railMeshCollider.sharedMesh = null;
            railMeshCollider.sharedMesh = railMesh;
        }

        GameObject baseObject = new GameObject("Endcap_Base");
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(baseObject, "Create Base Component");
#endif
        baseObject.transform.SetParent(endcapObject.transform, false);

        MeshFilter baseMeshFilter = baseObject.AddComponent<MeshFilter>();
        MeshRenderer baseMeshRenderer = baseObject.AddComponent<MeshRenderer>();
        MeshCollider baseMeshCollider = baseObject.AddComponent<MeshCollider>();
        baseMeshRenderer.sharedMaterial = _baseMaterial;

        Mesh baseMesh = new Mesh { name = "Endcap_Base_Mesh" };
        baseMesh.SetVertices(_baseMeshData.vertices);
        baseMesh.SetTriangles(_baseMeshData.triangles, 0);
        baseMesh.SetUVs(0, _baseMeshData.uvs);
        baseMesh.RecalculateNormals();
        baseMesh.RecalculateBounds();
        baseMeshFilter.sharedMesh = baseMesh;

        if (_baseMeshData.vertices != null && _baseMeshData.triangles != null &&
            _baseMeshData.vertices.Count >= 3 && _baseMeshData.triangles.Count >= 3)
        {
            baseMeshCollider.sharedMesh = null;
            baseMeshCollider.sharedMesh = baseMesh;
        }

        return endcapObject;
    }
}
