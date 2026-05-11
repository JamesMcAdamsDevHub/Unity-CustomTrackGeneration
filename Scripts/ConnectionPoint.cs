using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    public Transform parentObject = null;
    public ConnectionPoint connectedPoint = null;
    public Transform worldTransform = null;
    public bool isConnected = false;
    public string ID = "";
    public void Initialize(Transform parentObject, Transform worldTransform, string name)
    {
        this.parentObject = parentObject;
        this.worldTransform = worldTransform;
        this.ID = name;
    }

    public void ConnectPoint(ConnectionPoint other)
    {
        if (other == null || other == this) return;
        if (isConnected || other.isConnected) return;

        if (worldTransform == null) worldTransform = transform;
        if (other.worldTransform == null) other.worldTransform = other.transform;

        if (parentObject == null && transform.parent != null)
            parentObject = transform.parent;

        if (other.parentObject == null && other.transform.parent != null)
            other.parentObject = other.transform.parent;

        other.connectedPoint = this;
        connectedPoint = other;
        other.isConnected = true;
        isConnected = true;

        if (parentObject == null || other.parentObject == null) return;

        TrackGenerationOrchestrator track1 =
            parentObject.GetComponentInParent<TrackGenerationOrchestrator>();

        TrackGenerationOrchestrator track2 =
            other.parentObject.GetComponentInParent<TrackGenerationOrchestrator>();

        if (track1 == null || track2 == null) return;

        track1.ConnectionAttachedUpdate(ID);
        track2.ConnectionAttachedUpdate(other.ID);
    }

    private void OnEnable()
    {
        if (worldTransform == null)
            worldTransform = transform;

        if (parentObject == null && transform.parent != null)
            parentObject = transform.parent;
    }

    public void DisconnectPoint(ConnectionPoint other)
    {
        if (parentObject == null || other.parentObject == null) return;

        TrackGenerationOrchestrator track1 =
            parentObject.GetComponentInParent<TrackGenerationOrchestrator>();

        TrackGenerationOrchestrator track2 =
            other.parentObject.GetComponentInParent<TrackGenerationOrchestrator>();

        if (track1 == null || track2 == null) return;

        track1.ConnectionDettachedUpdate(ID);
        track2.ConnectionDettachedUpdate(other.ID);

        other.isConnected = false;
        isConnected = false;
        other.connectedPoint = null;
        connectedPoint = null;
    }
}
