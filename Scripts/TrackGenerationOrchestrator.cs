using UnityEngine;
using System.Drawing;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public abstract class TrackGenerationOrchestrator : MonoBehaviour
{
    [SerializeField] protected TrackGenerationSettings _settings = new TrackGenerationSettings();

    protected TrackConstraintsData _trackConstraintsData = new TrackConstraintsData();

    protected const int MAX_VERTS_PER_TRACK = 6000;
    protected const int VERTS_PER_RING = 20;
    protected const int RINGS_PER_TRACK = MAX_VERTS_PER_TRACK / VERTS_PER_RING;

    protected abstract string ROOT_NAME { get; }
    protected abstract void GenerateNewTrack();

    protected virtual void ConnectionUpdate(string ID) { } // Optional functionality

    public ConnectionPoint startConnection = null;
    public const string START_CONNECTION_ID = "Start_Connection";
    private float _snapDistance = 100f;

    protected virtual void Update()
    {
#if UNITY_EDITOR
        Transform root = transform.Find(ROOT_NAME);
        if (root == null)
            GenerateTrack();
#endif
    }

    protected virtual void OnValidate()
    {
        if (_settings == null)
            return;

        _settings.UpdateConstraints(this);
    }

    public void GenerateTrack()
    {
        if (_settings == null) return;

        _settings.CopyTo(_trackConstraintsData);

        if (transform.Find(ROOT_NAME) != null)
            DestroyPreviousTrack();

        GenerateNewTrack();
        GenerateStartConnectionPoint();
    }

    public void RefreshFromConfig()
    {
        if (_settings != null)
        {
            _settings.UpdateConstraints(this);
        }
    }

    protected Transform GetOrCreateRoot()
    {
        Transform existing = transform.Find(ROOT_NAME);
        if (existing != null)
            return existing;

        GameObject root = new GameObject(ROOT_NAME);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(root, "Create Track Root");
        Undo.SetTransformParent(root.transform, transform, "Attach root to parent");
#endif

        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        return root.transform;
    }

    protected void GenerateSpecifiedEndcap(LocalPointData point)
    {
#if UNITY_EDITOR
        Quaternion localRotation = Quaternion.LookRotation(point.localForward, point.localUp);
        TrackEndcapData endcapData = new TrackEndcapData(_trackConstraintsData);
        endcapData.GenerateEndcapData();
        TrackEndcap endcap = new TrackEndcap(_settings.railMaterial, _settings.baseMaterial, endcapData.railMeshData, endcapData.baseMeshData);
        GameObject endcapGO = endcap.Generate(point.localPosition, localRotation);
        Undo.RegisterCreatedObjectUndo(endcapGO, "Create Endcap");
        Transform root = GetOrCreateRoot();
        Undo.SetTransformParent(endcapGO.transform, root, "Attach endcap to root");
        endcapGO.transform.localScale = Vector3.one;
#endif
    }

    private void DestroyPreviousTrack()
    {
        Transform root = transform.Find(ROOT_NAME);

        if (root == null) return;

#if UNITY_EDITOR
        Undo.DestroyObjectImmediate(root.gameObject);
#endif
    }

    protected void CreateTrackSegment(TrackRingsData trackRingsData)
    {
#if UNITY_EDITOR
        TrackSegment trackSegment = new TrackSegment(
            _settings.deckMaterial,
            _settings.railMaterial,
            _settings.baseMaterial,
            trackRingsData.deckMeshData,
            trackRingsData.railMeshData,
            trackRingsData.baseMeshData
        );

        GameObject trackSegmentGO = trackSegment.Generate();
        Undo.RegisterCreatedObjectUndo(trackSegmentGO, "Create Track Segment");

        Transform root = GetOrCreateRoot();
        Undo.SetTransformParent(trackSegmentGO.transform, root, "Attach Track Segment to root");

        trackSegmentGO.transform.localPosition = Vector3.zero;
        trackSegmentGO.transform.localRotation = Quaternion.identity;
        trackSegmentGO.transform.localScale = Vector3.one;
#endif
    }

    private void GenerateStartConnectionPoint()
    {
        GenerateConnectionPoint(new LocalPointData (), START_CONNECTION_ID);
    }

    protected void GenerateConnectionPoint(LocalPointData localSpawnPoint, string name)
    {
#if UNITY_EDITOR
        GameObject connectionObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(connectionObject, "Create Start Connection");
        Transform root = GetOrCreateRoot();
        Undo.SetTransformParent(connectionObject.transform, root, "Attach Start Connection to root");
        ConnectionPoint connectionPoint = Undo.AddComponent<ConnectionPoint>(connectionObject);
        connectionObject.transform.localPosition = localSpawnPoint.localPosition;
        connectionObject.transform.localRotation = Quaternion.LookRotation(localSpawnPoint.localForward, localSpawnPoint.localUp);
        connectionObject.transform.localScale = Vector3.one;
        connectionPoint.Initialize(root, connectionObject.transform, name);

        if (name == START_CONNECTION_ID && startConnection == null)
        {
            startConnection = connectionPoint;
        }
#endif
        }

    public void TrySnap()
    {
        Transform root = GetOrCreateRoot();
        float shortestDistance = _snapDistance + 1;
        ConnectionPoint[] points = Object.FindObjectsByType<ConnectionPoint>(FindObjectsSortMode.None);
        ConnectionPoint closestPoint = null;
        foreach (ConnectionPoint point in points)
        {
            if (point.parentObject == root) continue;

            if (point.isConnected) continue;

            float distance = Vector3.Distance(startConnection.transform.position, point.worldTransform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestPoint = point;
            }
        }

        if (closestPoint == null || shortestDistance > _snapDistance)
            return;

        TrackGenerationOrchestrator connectionParent =
            closestPoint.parentObject.GetComponentInParent<TrackGenerationOrchestrator>();

        Quaternion desiredRotation;

        // If snapping to another track's start, face opposite direction
        if (connectionParent != null && connectionParent.startConnection == closestPoint)
        {
            desiredRotation = Quaternion.LookRotation(
                -closestPoint.worldTransform.forward,
                 closestPoint.worldTransform.up
            );
        }
        else
        {
            desiredRotation = closestPoint.worldTransform.rotation;
        }

        Quaternion deltaRotation =
            desiredRotation * Quaternion.Inverse(startConnection.worldTransform.rotation);

        transform.rotation = deltaRotation * transform.rotation;

        Vector3 deltaPosition =
            closestPoint.worldTransform.position - startConnection.worldTransform.position;

        transform.position += deltaPosition;

        closestPoint.connectedPoint = startConnection;
        startConnection.connectedPoint = closestPoint;
        closestPoint.isConnected = true;
        startConnection.isConnected = true;
    }

    private void OnDestroy()
    {
        DisconnectTracks();
    }

    public void DisconnectTracks()
    {
        ConnectionPoint[] points =
        GetComponentsInChildren<ConnectionPoint>();

        foreach (ConnectionPoint point in points) 
        {
            if (point.connectedPoint != null)
            {
                point.connectedPoint.isConnected = false;
                point.connectedPoint = null;
            }
            point.connectedPoint = null;
            point.isConnected = false;
        }
    }
}