using System;
using UnityEngine;

public class Track3WayIntersectionData : TrackIntersectionData
{
    private const float MIN_RAIL_RUN_LENGTH = 0.001f;
    private const float RAIL_WORLD_UNITS_PER_TILE = 0.6f;
    private LocalPointData _leftPoint;
    private LocalPointData _rightPoint;

    private struct ConnectionOpening
    {
        public BoundaryEndpoint leftEndpoint;
        public BoundaryEndpoint rightEndpoint;

        public ConnectionOpening(int index, LocalPointData point, float halfTrackWidth)
        {
            Vector3 forward = point.localForward.normalized;
            Vector3 up = point.localUp.normalized;
            Vector3 right = Vector3.Cross(forward, up).normalized;

            leftEndpoint = new BoundaryEndpoint(
                index,
                point.localPosition - right * halfTrackWidth,
                right
            );

            rightEndpoint = new BoundaryEndpoint(
                index,
                point.localPosition + right * halfTrackWidth,
                -right
            );
        }
    }

    private struct BoundaryEndpoint
    {
        public int connectionOpeningIndex;
        public Vector3 position;
        public Vector3 railInsetDirection;

        public BoundaryEndpoint(int connectionOpeningIndex, Vector3 position, Vector3 railInsetDirection)
        {
            this.connectionOpeningIndex = connectionOpeningIndex;
            this.position = position;
            this.railInsetDirection = railInsetDirection.normalized;
        }
    }

    public Track3WayIntersectionData(TrackConstraintsData trackConstraintsData) : base(trackConstraintsData)
    {
        _leftPoint = GetDefaultLeftConnectionPoint();
        _rightPoint = GetDefaultRightConnectionPoint();
    }

    public void GenerateIntersectionData()
    {
        ClearMeshData();

        ConnectionOpening[] connectionOpenings = GetConnectionOpenings(new LocalPointData(), _leftPoint, _rightPoint);
        BoundaryEndpoint[] boundary = GetOrderedBoundary(connectionOpenings);

        AddDeck(boundary);
        AddBase(boundary);
        AddRails(boundary);
    }

    private ConnectionOpening[] GetConnectionOpenings(LocalPointData startPoint, LocalPointData leftPoint, LocalPointData rightPoint)
    {
        float halfTrackWidth = _trackConstraintsData.TrackWidth / 2f;

        return new[]
        {
            new ConnectionOpening(0, startPoint, halfTrackWidth),
            new ConnectionOpening(1, leftPoint, halfTrackWidth),
            new ConnectionOpening(2, rightPoint, halfTrackWidth)
        };
    }

    private BoundaryEndpoint[] GetOrderedBoundary(ConnectionOpening[] connectionOpenings)
    {
        BoundaryEndpoint[] boundary =
        {
            connectionOpenings[0].leftEndpoint,
            connectionOpenings[0].rightEndpoint,
            connectionOpenings[1].leftEndpoint,
            connectionOpenings[1].rightEndpoint,
            connectionOpenings[2].leftEndpoint,
            connectionOpenings[2].rightEndpoint
        };

        Vector3 center = GetBoundaryCenter(boundary);
        Array.Sort(boundary, (a, b) => GetAngleAroundCenter(a.position, center).CompareTo(GetAngleAroundCenter(b.position, center)));

        if (GetSignedPolygonArea(boundary) < 0f)
            Array.Reverse(boundary);

        return boundary;
    }

    private Vector3 GetBoundaryCenter(BoundaryEndpoint[] boundary)
    {
        Vector3 center = Vector3.zero;

        for (int i = 0; i < boundary.Length; i++)
            center += boundary[i].position;

        return center / boundary.Length;
    }

    private float GetAngleAroundCenter(Vector3 position, Vector3 center)
    {
        Vector3 direction = position - center;
        return Mathf.Atan2(direction.z, direction.x);
    }

    private float GetSignedPolygonArea(BoundaryEndpoint[] boundary)
    {
        float area = 0f;

        for (int i = 0; i < boundary.Length; i++)
        {
            Vector3 current = boundary[i].position;
            Vector3 next = boundary[(i + 1) % boundary.Length].position;
            area += current.x * next.z - next.x * current.z;
        }

        return area;
    }

    private void AddDeck(BoundaryEndpoint[] boundary)
    {
        Vector3 trackHeight = Vector3.up * _trackConstraintsData.TrackHeight;
        Vector3 center = GetBoundaryCenter(boundary);

        for (int i = 0; i < boundary.Length; i++)
        {
            Vector3 current = boundary[i].position;
            Vector3 next = boundary[(i + 1) % boundary.Length].position;

            AddTriangle(
                deckMeshData,
                center + trackHeight,
                current + trackHeight,
                next + trackHeight,
                Vector3.up
            );
        }
    }

    private void AddBase(BoundaryEndpoint[] boundary)
    {
        float uvTileSize = Mathf.Max(_trackConstraintsData.BaseMaterialTileSize, 0.01f);
        float sideV = uvTileSize * _trackConstraintsData.TrackHeight / 20f;
        Vector3 trackHeight = Vector3.up * _trackConstraintsData.TrackHeight;

        for (int i = 0; i < boundary.Length; i++)
        {
            BoundaryEndpoint current = boundary[i];
            BoundaryEndpoint next = boundary[(i + 1) % boundary.Length];

            if (current.connectionOpeningIndex == next.connectionOpeningIndex)
                continue;

            float sideU = uvTileSize * Vector3.Distance(current.position, next.position) / 20f;
            AddQuad(baseMeshData, current.position + trackHeight, next.position + trackHeight, current.position, next.position, sideU, sideV);
        }

        AddBaseBottom(boundary);
    }

    private void AddBaseBottom(BoundaryEndpoint[] boundary)
    {
        Vector3 center = GetBoundaryCenter(boundary);

        for (int i = 0; i < boundary.Length; i++)
        {
            Vector3 current = boundary[i].position;
            Vector3 next = boundary[(i + 1) % boundary.Length].position;

            AddTriangle(
                baseMeshData,
                center,
                next,
                current,
                Vector3.down
            );
        }
    }

    private void AddRails(BoundaryEndpoint[] boundary)
    {
        float accumulatedDistance = 0f;

        for (int i = 0; i < boundary.Length; i++)
        {
            BoundaryEndpoint current = boundary[i];
            BoundaryEndpoint next = boundary[(i + 1) % boundary.Length];
            bool isConnectionOpeningEdge = current.connectionOpeningIndex == next.connectionOpeningIndex;

            if (isConnectionOpeningEdge)
                continue;

            float length = Vector3.Distance(current.position, next.position);
            if (length <= MIN_RAIL_RUN_LENGTH)
                continue;

            float startU = accumulatedDistance;
            accumulatedDistance += length;
            float endU = accumulatedDistance;

            AddRailRun(current, next, GetRailDistanceUv(startU), GetRailDistanceUv(endU));
        }
    }

    private void AddRailRun(BoundaryEndpoint start, BoundaryEndpoint end, float startU, float endU)
    {
        Vector3[] startSection = GetRailSectionAtEndpoint(start);
        Vector3[] endSection = GetRailSectionAtEndpoint(end);
        Vector3 path = (end.position - start.position).normalized;

        for (int i = 0; i < startSection.Length - 1; i++)
        {
            float startV = GetRailProfileV(i);
            float endV = GetRailProfileV(i + 1);
            Vector3 normal = GetRailFaceNormal(path, startSection[i], startSection[i + 1]);

            AddOrientedQuad(
                railMeshData,
                startSection[i + 1],
                endSection[i + 1],
                startSection[i],
                endSection[i],
                new Vector2(startU, endV),
                new Vector2(endU, endV),
                new Vector2(startU, startV),
                new Vector2(endU, startV),
                normal
            );
        }
    }

    private Vector3[] GetRailSectionAtEndpoint(BoundaryEndpoint endpoint)
    {
        float[] offsets = GetRailProfileOffsets();
        Vector3[] section = new Vector3[offsets.Length];

        for (int i = 0; i < offsets.Length; i++)
        {
            section[i] = endpoint.position
                + endpoint.railInsetDirection * offsets[i]
                + Vector3.up * GetRailProfileHeight(offsets[i]);
        }

        return section;
    }

    private Vector3 GetRailFaceNormal(Vector3 path, Vector3 outerPoint, Vector3 innerPoint)
    {
        Vector3 profileDirection = (innerPoint - outerPoint).normalized;
        Vector3 normal = Vector3.Cross(path, profileDirection).normalized;

        if (Vector3.Dot(normal, Vector3.up) < 0f)
            normal = -normal;

        return normal.sqrMagnitude <= 0.001f ? Vector3.up : normal;
    }

    private float GetRailProfileV(int index)
    {
        float[] offsets = GetRailProfileOffsets();
        float railWidth = Mathf.Max(_trackConstraintsData.RailWidth, 0.001f);

        return Mathf.Clamp01(offsets[index] / railWidth);
    }

    private float GetRailDistanceUv(float distance)
    {
        return _trackConstraintsData.RailMaterialTileSize * distance / RAIL_WORLD_UNITS_PER_TILE;
    }

    private LocalPointData GetDefaultLeftConnectionPoint()
    {
        Vector3 center = GetDefaultIntersectionCenter();
        Vector3 localPosition = center + Quaternion.AngleAxis(-120f, Vector3.up) * (Vector3.back * _trackConstraintsData.TrackWidth);
        Vector3 localForward = (localPosition - center).normalized;

        return new LocalPointData(localPosition, localForward, Vector3.up);
    }

    private LocalPointData GetDefaultRightConnectionPoint()
    {
        Vector3 center = GetDefaultIntersectionCenter();
        Vector3 localPosition = center + Quaternion.AngleAxis(120f, Vector3.up) * (Vector3.back * _trackConstraintsData.TrackWidth);
        Vector3 localForward = (localPosition - center).normalized;

        return new LocalPointData(localPosition, localForward, Vector3.up);
    }

    private Vector3 GetDefaultIntersectionCenter()
    {
        return Vector3.forward * _trackConstraintsData.TrackWidth;
    }

    private void AddTriangle(MeshData meshData, Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
    {
        int startIdx = meshData.vertices.Count;
        Vector3 triangleNormal = Vector3.Cross(b - a, c - a);

        if (Vector3.Dot(triangleNormal, normal) < 0f)
        {
            meshData.vertices.Add(a);
            meshData.vertices.Add(c);
            meshData.vertices.Add(b);
            meshData.uvs.Add(GetPlanarUv(a));
            meshData.uvs.Add(GetPlanarUv(c));
            meshData.uvs.Add(GetPlanarUv(b));
        }
        else
        {
            meshData.vertices.Add(a);
            meshData.vertices.Add(b);
            meshData.vertices.Add(c);
            meshData.uvs.Add(GetPlanarUv(a));
            meshData.uvs.Add(GetPlanarUv(b));
            meshData.uvs.Add(GetPlanarUv(c));
        }

        meshData.triangles.Add(startIdx);
        meshData.triangles.Add(startIdx + 1);
        meshData.triangles.Add(startIdx + 2);
    }

    private Vector2 GetPlanarUv(Vector3 vertex)
    {
        float width = Mathf.Max(_trackConstraintsData.TrackWidth, 0.001f);
        return new Vector2(vertex.x / width, vertex.z / width);
    }
}
