using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    [HideInInspector]
    public Transform parentObject = null;

    [HideInInspector]
    public ConnectionPoint connectedPoint = null;

    [HideInInspector]
    public Transform worldTransform = null;

    [HideInInspector]
    public bool isConnected = false;

    [HideInInspector]
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
        if (other == null) return;

        bool isReciprocalConnection = connectedPoint == other && other.connectedPoint == this;
        if (!isReciprocalConnection)
        {
            if (connectedPoint == other)
                connectedPoint = null;

            isConnected = connectedPoint != null;

            return;
        }

        TrackGenerationOrchestrator track1 = parentObject == null
            ? null
            : parentObject.GetComponentInParent<TrackGenerationOrchestrator>();

        TrackGenerationOrchestrator track2 = other.parentObject == null
            ? null
            : other.parentObject.GetComponentInParent<TrackGenerationOrchestrator>();

        string track1ConnectionID = ID;
        string track2ConnectionID = other.ID;

        other.isConnected = false;
        isConnected = false;
        other.connectedPoint = null;
        connectedPoint = null;

        if (track1 == null || track2 == null) return;

        track1.ConnectionDetachedUpdate(track1ConnectionID);
        track2.ConnectionDetachedUpdate(track2ConnectionID);
    }
}
