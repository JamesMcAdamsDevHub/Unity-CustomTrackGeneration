using UnityEditor;
using UnityEngine;



public class ConnectionPoint : MonoBehaviour
{
    public Transform parentObject = null;
    public ConnectionPoint connectedPoint = null;
    public Transform worldTransform;
    public bool isConnected = false;
    public string ID;
    public void Initialize(Transform parentObject, Transform worldTransform, string name)
    {
        this.parentObject = parentObject;
        this.worldTransform = worldTransform;
        this.ID = name;
    }
}
