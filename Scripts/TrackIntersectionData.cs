using UnityEngine;

public abstract class TrackIntersectionData
{
    protected TrackConstraintsData _trackConstraintsData;
    public MeshData deckMeshData = new MeshData();
    public MeshData railMeshData = new MeshData();
    public MeshData baseMeshData = new MeshData();

    protected TrackIntersectionData(TrackConstraintsData trackConstraintsData)
    {
        _trackConstraintsData = trackConstraintsData;
    }

    protected void ClearMeshData()
    {
        ClearMeshData(deckMeshData);
        ClearMeshData(railMeshData);
        ClearMeshData(baseMeshData);
    }

    protected void ClearMeshData(MeshData meshData)
    {
        meshData.vertices.Clear();
        meshData.triangles.Clear();
        meshData.uvs.Clear();
    }

    protected void AddQuad(MeshData meshData, Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, float uvWidth, float uvHeight)
    {
        meshData.vertices.Add(topLeft);
        meshData.vertices.Add(topRight);
        meshData.vertices.Add(bottomLeft);
        meshData.vertices.Add(bottomRight);

        AddRectangularSetOfTriangles(meshData);
        AddRectangularUVs(meshData, uvWidth, uvHeight);
    }

    protected void AddRectangularUVs(MeshData meshData, float uvWidth, float uvHeight)
    {
        meshData.uvs.Add(new Vector2(0f, uvHeight));
        meshData.uvs.Add(new Vector2(uvWidth, uvHeight));
        meshData.uvs.Add(new Vector2(0f, 0f));
        meshData.uvs.Add(new Vector2(uvWidth, 0f));
    }

    protected void AddRectangularSetOfTriangles(MeshData meshData)
    {
        int startIdx = meshData.vertices.Count - 4;
        meshData.triangles.Add(startIdx); meshData.triangles.Add(startIdx + 1); meshData.triangles.Add(startIdx + 2);
        meshData.triangles.Add(startIdx + 2); meshData.triangles.Add(startIdx + 1); meshData.triangles.Add(startIdx + 3);
    }

    protected void AddRailStrip(Vector3 start, Vector3 end, Vector3 inward, bool capStart, bool capEnd)
    {
        float length = Vector3.Distance(start, end);
        if (length <= 0.001f)
            return;

        Vector3 path = (end - start).normalized;
        Vector3[] section = GetRailSection(start, inward.normalized);
        Vector3[] nextSection = GetRailSection(end, inward.normalized);
        float uvLength = GetRailUvLength(length);

        AddRailFace(section[0], nextSection[0], section[1], nextSection[1], uvLength);
        AddRailFaceWithPathUvs(section[3], section[2], nextSection[3], nextSection[2], uvLength);

        if (_trackConstraintsData.useSplitRidge)
        {
            AddRailFaceWithPathUvs(section[5], section[4], nextSection[5], nextSection[4], uvLength);
        }

        if (capStart)
            AddRailEndCap(section, -path);

        if (capEnd)
            AddRailEndCap(nextSection, path);
    }

    protected Vector3[] GetRailSection(Vector3 origin, Vector3 inward)
    {
        float railWidth = _trackConstraintsData.RailWidth;
        GetRailProfileOffsets(out float outerOffset, out float innerOffset);

        Vector3 up = Vector3.up;
        Vector3 deckHeight = up * _trackConstraintsData.TrackHeight;
        Vector3 ridgeHeight = up * (_trackConstraintsData.TrackHeight + _trackConstraintsData.RailRidgeHeight);

        Vector3 outerBase = origin + deckHeight;
        Vector3 innerBase = origin + inward * railWidth + deckHeight;
        Vector3 innerRidge = origin + inward * innerOffset + ridgeHeight;
        Vector3 outerRidge = origin + inward * outerOffset + ridgeHeight;

        if (_trackConstraintsData.useSplitRidge)
        {
            return new[]
            {
                outerRidge, outerBase,
                innerBase, innerRidge,
                innerRidge, outerRidge
            };
        }

        return new[]
        {
            outerRidge, outerBase,
            innerBase, innerRidge
        };
    }

    protected float GetRailUvLength(float width)
    {
        return Mathf.Clamp(width / _trackConstraintsData.RailWidth * _trackConstraintsData.RailMaterialTileSize, 1f, 10f);
    }

    protected float GetRailUvScale()
    {
        return GetRailUvLength(_trackConstraintsData.RailWidth) / _trackConstraintsData.RailWidth;
    }

    protected float[] GetRailProfileOffsets()
    {
        float railWidth = _trackConstraintsData.RailWidth;
        GetRailProfileOffsets(out float outerOffset, out float innerOffset);

        if (_trackConstraintsData.useSplitRidge)
            return GetUniqueRailProfileOffsets(0f, outerOffset, innerOffset, railWidth);

        return new[] { 0f, innerOffset, railWidth };
    }

    protected float GetRailProfileHeight(float offsetFromOuter)
    {
        float railWidth = _trackConstraintsData.RailWidth;
        float deckY = _trackConstraintsData.TrackHeight;
        float ridgeY = _trackConstraintsData.TrackHeight + _trackConstraintsData.RailRidgeHeight;

        if (_trackConstraintsData.useSplitRidge)
        {
            GetRailProfileOffsets(out float outerOffset, out float innerOffset);

            if (Mathf.Approximately(outerOffset, 0f) && Mathf.Approximately(innerOffset, railWidth))
                return ridgeY;

            if (Mathf.Approximately(outerOffset, innerOffset))
                return offsetFromOuter <= outerOffset
                    ? Mathf.Lerp(deckY, ridgeY, Mathf.InverseLerp(0f, outerOffset, offsetFromOuter))
                    : Mathf.Lerp(ridgeY, deckY, Mathf.InverseLerp(innerOffset, railWidth, offsetFromOuter));

            if (offsetFromOuter <= outerOffset)
                return Mathf.Lerp(deckY, ridgeY, Mathf.InverseLerp(0f, outerOffset, offsetFromOuter));

            if (offsetFromOuter <= innerOffset)
                return ridgeY;

            return Mathf.Lerp(ridgeY, deckY, Mathf.InverseLerp(innerOffset, railWidth, offsetFromOuter));
        }

        float[] offsets = GetRailProfileOffsets();
        return offsetFromOuter <= offsets[1]
            ? Mathf.Lerp(deckY, ridgeY, Mathf.InverseLerp(offsets[0], offsets[1], offsetFromOuter))
            : Mathf.Lerp(ridgeY, deckY, Mathf.InverseLerp(offsets[1], railWidth, offsetFromOuter));
    }

    protected void GetRailProfileOffsets(out float outerOffset, out float innerOffset)
    {
        float railWidth = _trackConstraintsData.RailWidth;
        float innerOffsetFromInner = _trackConstraintsData.useSplitRidge
            ? railWidth / 2f - railWidth * _trackConstraintsData.RailRidgePosition / 2f
            : railWidth * _trackConstraintsData.RailRidgePosition;

        float outerOffsetFromInner = _trackConstraintsData.useSplitRidge
            ? railWidth / 2f + railWidth * _trackConstraintsData.RailRidgePosition / 2f
            : railWidth * _trackConstraintsData.RailRidgePosition;

        innerOffset = railWidth - innerOffsetFromInner;
        outerOffset = railWidth - outerOffsetFromInner;
    }

    protected void AddOrientedQuad(MeshData meshData, Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, Vector2 topLeftUv, Vector2 topRightUv, Vector2 bottomLeftUv, Vector2 bottomRightUv, Vector3 normal)
    {
        int startIdx = meshData.vertices.Count;
        Vector3 triangleNormal = Vector3.Cross(topRight - topLeft, bottomLeft - topLeft);

        meshData.vertices.Add(topLeft);
        meshData.uvs.Add(topLeftUv);

        if (Vector3.Dot(triangleNormal, normal) < 0f)
        {
            meshData.vertices.Add(bottomLeft);
            meshData.vertices.Add(topRight);
            meshData.vertices.Add(bottomRight);
            meshData.uvs.Add(bottomLeftUv);
            meshData.uvs.Add(topRightUv);
            meshData.uvs.Add(bottomRightUv);
        }
        else
        {
            meshData.vertices.Add(topRight);
            meshData.vertices.Add(bottomLeft);
            meshData.vertices.Add(bottomRight);
            meshData.uvs.Add(topRightUv);
            meshData.uvs.Add(bottomLeftUv);
            meshData.uvs.Add(bottomRightUv);
        }

        meshData.triangles.Add(startIdx);
        meshData.triangles.Add(startIdx + 1);
        meshData.triangles.Add(startIdx + 2);
        meshData.triangles.Add(startIdx + 2);
        meshData.triangles.Add(startIdx + 1);
        meshData.triangles.Add(startIdx + 3);
    }

    private void AddRailFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float uvLength)
    {
        AddQuad(railMeshData, a, b, c, d, uvLength, 1f);
    }

    private void AddRailFaceWithPathUvs(Vector3 startTop, Vector3 startBottom, Vector3 endTop, Vector3 endBottom, float uvLength)
    {
        railMeshData.vertices.Add(startTop);
        railMeshData.vertices.Add(startBottom);
        railMeshData.vertices.Add(endTop);
        railMeshData.vertices.Add(endBottom);

        AddRectangularSetOfTriangles(railMeshData);

        railMeshData.uvs.Add(new Vector2(0f, 1f));
        railMeshData.uvs.Add(new Vector2(0f, 0f));
        railMeshData.uvs.Add(new Vector2(uvLength, 1f));
        railMeshData.uvs.Add(new Vector2(uvLength, 0f));
    }

    private void AddRailEndCap(Vector3[] section, Vector3 normal)
    {
        if (_trackConstraintsData.useSplitRidge)
        {
            AddRailEndTriangle(section[0], section[1], section[5], normal);
            AddRailEndTriangle(section[5], section[1], section[4], normal);
            AddRailEndTriangle(section[4], section[1], section[2], normal);
            AddRailEndTriangle(section[4], section[2], section[3], normal);
        }
        else
        {
            AddRailEndTriangle(section[0], section[1], section[2], normal);
            AddRailEndTriangle(section[0], section[2], section[3], normal);
        }
    }

    private void AddRailEndTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
    {
        int startIdx = railMeshData.vertices.Count;
        Vector3 triangleNormal = Vector3.Cross(b - a, c - a);

        if (Vector3.Dot(triangleNormal, normal) < 0f)
        {
            railMeshData.vertices.Add(a);
            railMeshData.vertices.Add(c);
            railMeshData.vertices.Add(b);
        }
        else
        {
            railMeshData.vertices.Add(a);
            railMeshData.vertices.Add(b);
            railMeshData.vertices.Add(c);
        }

        railMeshData.triangles.Add(startIdx);
        railMeshData.triangles.Add(startIdx + 1);
        railMeshData.triangles.Add(startIdx + 2);

        railMeshData.uvs.Add(new Vector2(0f, 0f));
        railMeshData.uvs.Add(new Vector2(1f, 0f));
        railMeshData.uvs.Add(new Vector2(0f, 1f));
    }

    private float[] GetUniqueRailProfileOffsets(params float[] offsets)
    {
        const float MIN_OFFSET_DELTA = 0.0001f;
        System.Array.Sort(offsets);

        int count = 0;
        for (int i = 0; i < offsets.Length; i++)
        {
            if (count > 0 && Mathf.Abs(offsets[i] - offsets[count - 1]) < MIN_OFFSET_DELTA)
                continue;

            offsets[count] = offsets[i];
            count++;
        }

        float[] uniqueOffsets = new float[count];
        for (int i = 0; i < count; i++)
            uniqueOffsets[i] = offsets[i];

        return uniqueOffsets;
    }
}
