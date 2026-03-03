using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

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

    public GameObject Generate(Vector3 worldPosition, Quaternion worldRotation)
    {
        GameObject endcapObject;
        endcapObject = new GameObject("Endcap");
        endcapObject.transform.SetPositionAndRotation(worldPosition, worldRotation);
        endcapObject.transform.localScale = Vector3.one;

        GameObject railObject;
        railObject = new GameObject("Endcap_Rail");
        railObject.transform.SetParent(endcapObject.transform, false);
        MeshFilter railMeshFilter = railObject.AddComponent<MeshFilter>();
        MeshRenderer railMeshRenderer = railObject.AddComponent<MeshRenderer>();
        MeshCollider railMeshCollider = railObject.AddComponent<MeshCollider>();
        railMeshRenderer.sharedMaterial = _railMaterial;

        Mesh railMesh = new Mesh();
        railMesh.name = "Endcap_Rail_Mesh";
        railMeshFilter.sharedMesh = railMesh;
        railMesh.SetVertices(_railMeshData.vertices);
        railMesh.SetTriangles(_railMeshData.triangles, 0);
        railMesh.SetUVs(0, _railMeshData.uvs);
        railMesh.RecalculateNormals();
        railMesh.RecalculateBounds();
        railMeshCollider.sharedMesh = railMesh;

        GameObject baseObject;
        baseObject = new GameObject("Endcap_Base");
        baseObject.transform.SetParent(endcapObject.transform, false);
        MeshFilter baseMeshFilter = baseObject.AddComponent<MeshFilter>();
        MeshRenderer baseMeshRenderer = baseObject.AddComponent<MeshRenderer>();
        MeshCollider baseMeshCollider = baseObject.AddComponent<MeshCollider>();
        baseMeshRenderer.sharedMaterial = _baseMaterial;

        Mesh baseMesh = new Mesh();
        baseMesh.name = "Endcap_Base_Mesh";
        baseMeshFilter.sharedMesh = baseMesh;
        baseMesh.SetVertices(_baseMeshData.vertices);
        baseMesh.SetTriangles(_baseMeshData.triangles, 0);
        baseMesh.SetUVs(0, _baseMeshData.uvs);
        baseMesh.RecalculateNormals();
        baseMesh.RecalculateBounds();
        baseMeshCollider.sharedMesh = baseMesh;

        return endcapObject; 
    }
}
