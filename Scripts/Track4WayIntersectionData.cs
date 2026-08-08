using UnityEngine;

public class Track4WayIntersectionData
{
    private TrackConstraintsData _trackConstraintsData;
    public MeshData deckMeshData = new MeshData();
    public MeshData railMeshData = new MeshData();
    public MeshData baseMeshData = new MeshData();

    public Track4WayIntersectionData(TrackConstraintsData trackConstraintsData)
    {
        this._trackConstraintsData = trackConstraintsData;
    }

    public void GenerateIntersectionData(bool southOpen, bool northOpen, bool eastOpen, bool westOpen)
    {
        deckMeshData.vertices.Clear();
        deckMeshData.triangles.Clear();
        deckMeshData.uvs.Clear();
        railMeshData.vertices.Clear();
        railMeshData.triangles.Clear();
        railMeshData.uvs.Clear();
        baseMeshData.vertices.Clear();
        baseMeshData.triangles.Clear();
        baseMeshData.uvs.Clear();

        // Dimension Variables
        Vector3 forward = Vector3.forward;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.right;

        Vector3 trackWidthFromCenter = right * (_trackConstraintsData.TrackWidth / 2);
        Vector3 trackHeight = up * _trackConstraintsData.TrackHeight;
        Vector3 trackLength = forward * _trackConstraintsData.TrackWidth;

        // Deck //
        deckMeshData.vertices.Add(-trackWidthFromCenter + trackHeight);
        deckMeshData.vertices.Add(-trackWidthFromCenter + trackLength + trackHeight);
        deckMeshData.vertices.Add(trackWidthFromCenter + trackHeight);

        deckMeshData.vertices.Add(trackWidthFromCenter + trackHeight);
        deckMeshData.vertices.Add(-trackWidthFromCenter + trackLength + trackHeight);
        deckMeshData.vertices.Add(trackWidthFromCenter + trackLength + trackHeight);

        for (int i = 0; i < 6; i++)
        {
            deckMeshData.triangles.Add(i);
        }

        deckMeshData.uvs.Add(new Vector2(0, 0));
        deckMeshData.uvs.Add(new Vector2(0, 1));
        deckMeshData.uvs.Add(new Vector2(1, 0));
        deckMeshData.uvs.Add(new Vector2(1, 0));
        deckMeshData.uvs.Add(new Vector2(0, 1));
        deckMeshData.uvs.Add(new Vector2(1, 1));

        // BASE //
        float uvTileSize = Mathf.Max(_trackConstraintsData.BaseMaterialTileSize, 0.01f);
        float sideU = uvTileSize * _trackConstraintsData.TrackWidth / 20f;
        float sideV = uvTileSize * _trackConstraintsData.TrackHeight / 20f;
        float bottomU = uvTileSize * _trackConstraintsData.TrackWidth / 20f;
        float bottomV = uvTileSize * _trackConstraintsData.TrackWidth / 20f;

        Vector3 frontLeftBottom = -trackWidthFromCenter;
        Vector3 frontRightBottom = trackWidthFromCenter;
        Vector3 backLeftBottom = -trackWidthFromCenter + trackLength;
        Vector3 backRightBottom = trackWidthFromCenter + trackLength;

        Vector3 frontLeftTop = frontLeftBottom + trackHeight;
        Vector3 frontRightTop = frontRightBottom + trackHeight;
        Vector3 backLeftTop = backLeftBottom + trackHeight;
        Vector3 backRightTop = backRightBottom + trackHeight;

        AddQuad(
            baseMeshData,
            frontLeftTop, frontLeftBottom, backLeftTop, backLeftBottom,
            sideU, sideV
        );

        AddQuad(
            baseMeshData,
            frontRightTop, backRightTop, frontRightBottom, backRightBottom,
            sideU, sideV
        );

        AddQuad(
            baseMeshData,
            frontLeftTop, frontRightTop, frontLeftBottom, frontRightBottom,
            sideU, sideV
        );

        AddQuad(
            baseMeshData,
            backLeftTop, backLeftBottom, backRightTop, backRightBottom,
            sideU, sideV
        );

        AddQuad(
            baseMeshData,
            frontLeftBottom, frontRightBottom, backLeftBottom, backRightBottom,
            bottomU, bottomV
        );

        // RAIL //
        float halfWidth = _trackConstraintsData.TrackWidth / 2f;
        float railWidth = Mathf.Min(_trackConstraintsData.RailWidth, _trackConstraintsData.TrackWidth / 2f);
        float minX = -halfWidth;
        float maxX = halfWidth;
        float minZ = 0f;
        float maxZ = _trackConstraintsData.TrackWidth;

        if (!southOpen)
        {
            Vector3 southStart = new Vector3(minX + railWidth, 0f, minZ);
            Vector3 southEnd = new Vector3(maxX - railWidth, 0f, minZ);
            AddRailStrip(
                southStart,
                southEnd,
                Vector3.forward,
                false,
                false
            );
        }

        if (!northOpen)
        {
            Vector3 northStart = new Vector3(maxX - railWidth, 0f, maxZ);
            Vector3 northEnd = new Vector3(minX + railWidth, 0f, maxZ);
            AddRailStrip(
                northStart,
                northEnd,
                Vector3.back,
                false,
                false
            );
        }

        if (!eastOpen)
        {
            Vector3 eastStart = new Vector3(maxX, 0f, minZ + railWidth);
            Vector3 eastEnd = new Vector3(maxX, 0f, maxZ - railWidth);
            AddRailStrip(
                eastStart,
                eastEnd,
                Vector3.left,
                false,
                false
            );
        }

        if (!westOpen)
        {
            Vector3 westStart = new Vector3(minX, 0f, maxZ - railWidth);
            Vector3 westEnd = new Vector3(minX, 0f, minZ + railWidth);
            AddRailStrip(
                westStart,
                westEnd,
                Vector3.right,
                false,
                false
            );
        }

        AddRailCorner(new Vector3(minX, 0f, minZ), Vector3.right, Vector3.forward, westOpen, southOpen);
        AddRailCorner(new Vector3(maxX, 0f, minZ), Vector3.left, Vector3.forward, eastOpen, southOpen);
        AddRailCorner(new Vector3(minX, 0f, maxZ), Vector3.right, Vector3.back, westOpen, northOpen);
        AddRailCorner(new Vector3(maxX, 0f, maxZ), Vector3.left, Vector3.back, eastOpen, northOpen);
    }


    private void AddQuad(MeshData meshData, Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, float uvWidth, float uvHeight)
    {
        meshData.vertices.Add(topLeft);
        meshData.vertices.Add(topRight);
        meshData.vertices.Add(bottomLeft);
        meshData.vertices.Add(bottomRight);

        AddRectangularSetOfTriangles(meshData);
        AddRectangularUVs(meshData, uvWidth, uvHeight);
    }

    private void AddRectangularUVs(MeshData meshData, float uvWidth, float uvHeight)
    {
        meshData.uvs.Add(new Vector2(0f, uvHeight));
        meshData.uvs.Add(new Vector2(uvWidth, uvHeight));
        meshData.uvs.Add(new Vector2(0f, 0f));
        meshData.uvs.Add(new Vector2(uvWidth, 0f));
    }

    private void AddRectangularSetOfTriangles(MeshData meshData)
    {
        int startIdx = meshData.vertices.Count - 4;
        meshData.triangles.Add(startIdx); meshData.triangles.Add(startIdx + 1); meshData.triangles.Add(startIdx + 2);
        meshData.triangles.Add(startIdx + 2); meshData.triangles.Add(startIdx + 1); meshData.triangles.Add(startIdx + 3);
    }

    private void AddRailStrip(Vector3 start, Vector3 end, Vector3 inward, bool capStart, bool capEnd)
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

    private Vector3[] GetRailSection(Vector3 origin, Vector3 inward)
    {
        float railWidth = _trackConstraintsData.RailWidth;
        float innerOffsetFromInner = _trackConstraintsData.useSplitRidge
            ? railWidth / 2f - railWidth * _trackConstraintsData.RailRidgePosition / 2f
            : railWidth * _trackConstraintsData.RailRidgePosition;

        float outerOffsetFromInner = _trackConstraintsData.useSplitRidge
            ? railWidth / 2f + railWidth * _trackConstraintsData.RailRidgePosition / 2f
            : railWidth * _trackConstraintsData.RailRidgePosition;

        float innerOffset = railWidth - innerOffsetFromInner;
        float outerOffset = railWidth - outerOffsetFromInner;

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

    private float GetRailUvLength(float width)
    {
        return Mathf.Clamp(width / _trackConstraintsData.RailWidth * _trackConstraintsData.RailMaterialTileSize, 1f, 10f);
    }

    private float GetRailUvScale()
    {
        return GetRailUvLength(_trackConstraintsData.RailWidth) / _trackConstraintsData.RailWidth;
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

    private void AddRailCorner(Vector3 origin, Vector3 inwardA, Vector3 inwardB, bool sideAOpen, bool sideBOpen)
    {
        float railWidth = _trackConstraintsData.RailWidth;

        if (sideAOpen && !sideBOpen)
        {
            AddOneOpenRailCornerStrip(origin, inwardA.normalized, inwardB.normalized, railWidth);
            return;
        }

        if (!sideAOpen && sideBOpen)
        {
            AddOneOpenRailCornerStrip(origin, inwardB.normalized, inwardA.normalized, railWidth);
            return;
        }

        float[] offsets = GetRailProfileOffsets();
        float uvScale = GetRailUvScale();
        bool flipA = ShouldFlipCornerUvAxis(inwardA);
        bool flipB = ShouldFlipCornerUvAxis(inwardB);

        for (int a = 0; a < offsets.Length - 1; a++)
        {
            for (int b = 0; b < offsets.Length - 1; b++)
            {
                Vector3 p00 = GetRailCornerPoint(origin, inwardA, inwardB, offsets[a], offsets[b], sideAOpen, sideBOpen);
                Vector3 p10 = GetRailCornerPoint(origin, inwardA, inwardB, offsets[a + 1], offsets[b], sideAOpen, sideBOpen);
                Vector3 p01 = GetRailCornerPoint(origin, inwardA, inwardB, offsets[a], offsets[b + 1], sideAOpen, sideBOpen);
                Vector3 p11 = GetRailCornerPoint(origin, inwardA, inwardB, offsets[a + 1], offsets[b + 1], sideAOpen, sideBOpen);
                Vector2 uv00 = GetRailCornerUv(offsets[a], offsets[b], railWidth, uvScale, flipA, flipB);
                Vector2 uv10 = GetRailCornerUv(offsets[a + 1], offsets[b], railWidth, uvScale, flipA, flipB);
                Vector2 uv01 = GetRailCornerUv(offsets[a], offsets[b + 1], railWidth, uvScale, flipA, flipB);
                Vector2 uv11 = GetRailCornerUv(offsets[a + 1], offsets[b + 1], railWidth, uvScale, flipA, flipB);

                AddOrientedQuad(
                    railMeshData,
                    p01,
                    p11,
                    p00,
                    p10,
                    uv01,
                    uv11,
                    uv00,
                    uv10,
                    Vector3.up
                );
            }
        }

        if (IsFullWidthSplitRidge())
            AddFullWidthSplitCornerSides(origin, inwardA.normalized, inwardB.normalized);
    }

    private void AddOneOpenRailCornerStrip(Vector3 origin, Vector3 openInward, Vector3 closedSideInward, float railWidth)
    {
        Vector3 start = origin;
        Vector3 end = origin + openInward * railWidth;
        Vector3 expectedPath = GetClosedSideRailPathDirection(closedSideInward);

        if (Vector3.Dot((end - start).normalized, expectedPath) < 0f)
        {
            Vector3 originalStart = start;
            start = end;
            end = originalStart;
        }

        AddRailStrip(
            start,
            end,
            closedSideInward,
            false,
            false
        );
    }

    private Vector3 GetClosedSideRailPathDirection(Vector3 inward)
    {
        if (inward.z > 0.5f)
            return Vector3.right;

        if (inward.z < -0.5f)
            return Vector3.left;

        if (inward.x < -0.5f)
            return Vector3.forward;

        return Vector3.back;
    }

    private bool ShouldFlipCornerUvAxis(Vector3 inward)
    {
        return inward.x < -0.5f || inward.z < -0.5f;
    }

    private Vector2 GetRailCornerUv(float offsetA, float offsetB, float railWidth, float uvScale, bool flipA, bool flipB)
    {
        float u = flipA ? railWidth - offsetA : offsetA;
        float v = flipB ? railWidth - offsetB : offsetB;
        return new Vector2(u * uvScale, v * uvScale);
    }

    private bool IsFullWidthSplitRidge()
    {
        if (!_trackConstraintsData.useSplitRidge)
            return false;

        GetSplitRailProfileOffsets(out float outerOffset, out float innerOffset);
        return Mathf.Approximately(outerOffset, 0f) && Mathf.Approximately(innerOffset, _trackConstraintsData.RailWidth);
    }

    private void AddFullWidthSplitCornerSides(Vector3 origin, Vector3 inwardA, Vector3 inwardB)
    {
        float railWidth = _trackConstraintsData.RailWidth;

        AddRailCornerSideFace(origin, inwardA, inwardB, 0f, 0f, 0f, railWidth, -inwardA);
        AddRailCornerSideFace(origin, inwardA, inwardB, 0f, 0f, railWidth, 0f, -inwardB);
        AddRailCornerSideFace(origin, inwardA, inwardB, railWidth, 0f, railWidth, railWidth, inwardA);
        AddRailCornerSideFace(origin, inwardA, inwardB, 0f, railWidth, railWidth, railWidth, inwardB);
    }

    private void AddRailCornerSideFace(Vector3 origin, Vector3 inwardA, Vector3 inwardB, float startA, float startB, float endA, float endB, Vector3 normal)
    {
        float uvLength = GetRailUvLength(Vector2.Distance(new Vector2(startA, startB), new Vector2(endA, endB)));

        Vector3 startTop = GetRailCornerPoint(origin, inwardA, inwardB, startA, startB, false, false);
        Vector3 endTop = GetRailCornerPoint(origin, inwardA, inwardB, endA, endB, false, false);
        Vector3 startBottom = origin + inwardA * startA + inwardB * startB + Vector3.up * _trackConstraintsData.TrackHeight;
        Vector3 endBottom = origin + inwardA * endA + inwardB * endB + Vector3.up * _trackConstraintsData.TrackHeight;

        AddOrientedQuad(
            railMeshData,
            startTop,
            endTop,
            startBottom,
            endBottom,
            new Vector2(0f, 1f),
            new Vector2(uvLength, 1f),
            new Vector2(0f, 0f),
            new Vector2(uvLength, 0f),
            normal
        );
    }

    private float[] GetRailProfileOffsets()
    {
        float railWidth = _trackConstraintsData.RailWidth;
        float innerOffsetFromInner = _trackConstraintsData.useSplitRidge
            ? railWidth / 2f - railWidth * _trackConstraintsData.RailRidgePosition / 2f
            : railWidth * _trackConstraintsData.RailRidgePosition;

        float outerOffsetFromInner = _trackConstraintsData.useSplitRidge
            ? railWidth / 2f + railWidth * _trackConstraintsData.RailRidgePosition / 2f
            : railWidth * _trackConstraintsData.RailRidgePosition;

        float innerOffset = railWidth - innerOffsetFromInner;
        float outerOffset = railWidth - outerOffsetFromInner;

        if (_trackConstraintsData.useSplitRidge)
            return GetUniqueRailProfileOffsets(0f, outerOffset, innerOffset, railWidth);

        return new[] { 0f, innerOffset, railWidth };
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

    private Vector3 GetRailCornerPoint(Vector3 origin, Vector3 inwardA, Vector3 inwardB, float offsetA, float offsetB, bool sideAOpen, bool sideBOpen)
    {
        float railWidth = _trackConstraintsData.RailWidth;
        float profileOffset;

        if (!sideAOpen && !sideBOpen)
            profileOffset = Mathf.Min(offsetA, offsetB);
        else if (sideAOpen && sideBOpen)
            profileOffset = Mathf.Min(railWidth - offsetA, railWidth - offsetB);
        else if (sideAOpen)
            profileOffset = GetDominantProfileOffset(railWidth - offsetA, offsetB);
        else
            profileOffset = GetDominantProfileOffset(offsetA, railWidth - offsetB);

        return origin
            + inwardA.normalized * offsetA
            + inwardB.normalized * offsetB
            + Vector3.up * GetRailProfileHeight(profileOffset);
    }

    private float GetDominantProfileOffset(float firstOffset, float secondOffset)
    {
        float firstHeight = GetRailProfileHeight(firstOffset);
        float secondHeight = GetRailProfileHeight(secondOffset);

        return firstHeight >= secondHeight ? firstOffset : secondOffset;
    }

    private float GetRailProfileHeight(float offsetFromOuter)
    {
        float railWidth = _trackConstraintsData.RailWidth;
        float deckY = _trackConstraintsData.TrackHeight;
        float ridgeY = _trackConstraintsData.TrackHeight + _trackConstraintsData.RailRidgeHeight;

        if (_trackConstraintsData.useSplitRidge)
        {
            GetSplitRailProfileOffsets(out float outerOffset, out float innerOffset);

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

    private void GetSplitRailProfileOffsets(out float outerOffset, out float innerOffset)
    {
        float railWidth = _trackConstraintsData.RailWidth;
        float innerOffsetFromInner = railWidth / 2f - railWidth * _trackConstraintsData.RailRidgePosition / 2f;
        float outerOffsetFromInner = railWidth / 2f + railWidth * _trackConstraintsData.RailRidgePosition / 2f;

        innerOffset = railWidth - innerOffsetFromInner;
        outerOffset = railWidth - outerOffsetFromInner;
    }

    private void AddOrientedQuad(MeshData meshData, Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, Vector2 topLeftUv, Vector2 topRightUv, Vector2 bottomLeftUv, Vector2 bottomRightUv, Vector3 normal)
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

}
