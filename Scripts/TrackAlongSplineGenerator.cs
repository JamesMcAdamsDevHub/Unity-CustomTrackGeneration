using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackAlongSplineGenerator : MonoBehaviour
{
    [Header("Generate New Track")]
    [SerializeField, Tooltip("Click this to update track along spline.")] private bool GenerateTrack;

    [Header("Default Track Configuration")]
    [SerializeField, Tooltip("If enabled, constraints and materials will be overwritten by config values")] private bool _useConfig;
    [SerializeField] private TrackGenerationDefaultConfig _trackConfig;

    [Header("Track Constraints")]
    [SerializeField, Range(10f, 300f)] private float _trackWidth;
    [SerializeField, Range(1f, 10f)] private float _trackHeight;
    [SerializeField, Range(1f, 30f), Tooltip("Constrained: Max of 10% of trackWidth")] private float _railWidth;
    [SerializeField, Range(0f, 30f), Tooltip("Height of railRidge above track. Constrained: Max of trackHeight")] private float _railRidgeHeight;
    [SerializeField, Range(0f, 1f), Tooltip("Ridge Position [0,1]. 0 = Vertical inner edge, 1 = Vertical outer edge")] private float _railRidgePosition;
    [SerializeField, Range(0.5f, 200f), Tooltip("Smaller Distance = Smoother Track")] private float _distanceBetweenRings;

    [Header("Track Materials")]
    [SerializeField] private Material _deckMaterial;
    [SerializeField] private Material _railMaterial;
    [SerializeField] private Material _baseMaterial;

    [Header("Endcap Generation")]
    [SerializeField, Tooltip("Generate an endcap at the start of the spline.")] private bool _generateStartEndcap;
    [SerializeField, Tooltip("Generate an endcap at the end of the spline.")] private bool _generateEndEndcap;

    [Header("Spline Container")]
    [SerializeField] private SplineContainer _splineContainer;

    private TrackConstraintsData _trackConstraintsData = new TrackConstraintsData();
    private List<GameObject> _generatedGameObjects = new List<GameObject>();

#if UNITY_EDITOR
    private void OnValidate() 
    {
        if (_useConfig)
        {
            if (_trackConfig == null)
            {
                Debug.LogWarning("Could not access _trackConfig: _trackConfig is not assigned.", this);
                return;
            }

            UpdateLocalFieldsFromConfig();
        }
        else
        {
            EnforceLocalFieldConstraints();
        }

        if (GenerateTrack)
        {
            GenerateTrack = false;

            EditorApplication.delayCall += () =>
            {
                if (this == null) return;

                UpdateTrackConstraintsData();
                GenerateNewTrack();
            };
            
        }
    }
#endif
    private void EnforceLocalFieldConstraints()
    {
        _trackWidth = Mathf.Clamp(_trackWidth, 10f, 300f);
        _trackHeight = Mathf.Clamp(_trackHeight, 1f, 10f);

        _railWidth = Mathf.Max(1f, _railWidth);
        _railWidth = Mathf.Min(_trackWidth * 0.1f, _railWidth);
        _railRidgeHeight = Mathf.Max(0f, _railRidgeHeight);
        _railRidgeHeight = Mathf.Min(_trackHeight, _railRidgeHeight);

        _railRidgePosition = Mathf.Clamp(_railRidgePosition, 0f, 1f);
        _distanceBetweenRings = Mathf.Clamp(_distanceBetweenRings, 0.5f, 200f);
    }
    private void UpdateTrackConstraintsData()
    {
        _trackConstraintsData.TrackWidth = _trackWidth;
        _trackConstraintsData.TrackHeight = _trackHeight;
        _trackConstraintsData.RailRidgePosition = _railRidgePosition;
        _trackConstraintsData.DistanceBetweenRings = _distanceBetweenRings;
        _trackConstraintsData.RailWidth = _railWidth;
        _trackConstraintsData.RailRidgeHeight = _railRidgeHeight;
    }

    private void UpdateLocalFieldsFromConfig()
    {
        // Dev Note: _trackConfig applies all constraints
        _trackWidth = _trackConfig.TrackWidth;
        _trackHeight = _trackConfig.TrackHeight;
        _railRidgePosition = _trackConfig.RailRidgePosition;
        _distanceBetweenRings = _trackConfig.DistanceBetweenRings;
        _railWidth = _trackConfig.RailWidth; 
        _railRidgeHeight = _trackConfig.RailRidgeHeight; 

        if (_trackConfig.DeckMaterial == null || _trackConfig.RailMaterial == null || _trackConfig.BaseMaterial == null)
        {
            Debug.LogWarning("Could not assign materials from _trackConfig: _trackConfig material(s) are not assigned.", this);
            return;
        }
        
        _deckMaterial = _trackConfig.DeckMaterial;
        _railMaterial = _trackConfig.RailMaterial;
        _baseMaterial = _trackConfig.BaseMaterial;
    }

    private void GenerateNewTrack()
    {
        if (_splineContainer == null)
        {
            Debug.LogWarning("Could not generate track: SplineContainer not assigned.", this);
            return;
        }
        if (_deckMaterial == null || _railMaterial == null || _baseMaterial == null)
        {
            Debug.LogWarning("Could not generate track: Material(s) not assigned.", this);
            return;
        }

        DestroyPreviousTrack();
        GenerateEndcaps();
        GenerateTrackAlongSpline();
    }

    private void DestroyPreviousTrack()
    {
        foreach (GameObject go in _generatedGameObjects)
        {
            if (go != null) 
                DestroyImmediate(go);
        }
        _generatedGameObjects.Clear();
    }

    private void GenerateEndcaps() 
    {
        if (_generateStartEndcap)
        {
            GenerateSpecifiedEndcap(true);
        }

        if (_generateEndEndcap)
        {
            GenerateSpecifiedEndcap(false);
        }
    }

    private void GenerateSpecifiedEndcap(bool isStartEndcap)
    {
        float3 posTemp, tanTemp, upTemp;
        float t = isStartEndcap ? 0f : 1f;
        _splineContainer.Evaluate(t, out posTemp, out tanTemp, out upTemp);

        Vector3 worldPosition = (Vector3)posTemp;

        Vector3 forward = ((Vector3)tanTemp).normalized;
        Vector3 up = ((Vector3)upTemp).normalized;

        if (!isStartEndcap) forward *= -1f;

        Quaternion worldRotation = Quaternion.LookRotation(forward, up);

        TrackEndcapData endcapData = new TrackEndcapData(_trackConstraintsData);
        endcapData.GenerateEndcapData();

        TrackEndcap endcap = new TrackEndcap(_railMaterial, _baseMaterial, endcapData.railMeshData, endcapData.baseMeshData);

        GameObject endcapGO = endcap.Generate(worldPosition, worldRotation);
        endcapGO.transform.SetParent(transform, true);

        _generatedGameObjects.Add(endcapGO);
    }

    private void GenerateTrackAlongSpline() 
    {
        /*  
         *  TODO: Determine # of track segments that must be created to
         *  assure no track segment contains more than 6000 vertices.
         *  Determine length of each segment, cutting last segment short if necessary to complete the generation.
         *  For each track segment:
         *      Create instance of TrackRingsData
         *      Call GenerateRingAtPoint on TrackRingsData using Transform data from spline point at interval
         *      Create insatnce of TrackSegment
         *      Calling Generate on TrackSegment and store it's returned GameObject in generatedGameObjects
        */
    }
}
