using UnityEngine;

public class Track4WayIntersectionData : TrackIntersectionData
{
    public Track4WayIntersectionData(TrackConstraintsData trackConstraintsData) : base(trackConstraintsData)
    {
    }

    public void GenerateIntersectionData(bool southOpen, bool northOpen, bool eastOpen, bool westOpen)
    {
        ClearMeshData();

        // Dimension Variables
        Vector3 forward = Vector3.forward;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.right;

        Vector3 trackWidthFromCenter = right * (_trackConstraintsData.TrackWidth / 2);
        Vector3 trackHeight = up * _trackConstraintsData.TrackHeight;
        Vector3 trackLength = forward * _trackConstraintsData.TrackWidth;

        AddDeck(trackWidthFromCenter, trackHeight, trackLength);
        AddBase(trackWidthFromCenter, trackHeight, trackLength);
        AddRails(southOpen, northOpen, eastOpen, westOpen);
    }

    private void AddDeck(Vector3 trackWidthFromCenter, Vector3 trackHeight, Vector3 trackLength)
    {
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
    }

    private void AddBase(Vector3 trackWidthFromCenter, Vector3 trackHeight, Vector3 trackLength)
    {
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

        AddQuad(baseMeshData, frontLeftTop, frontLeftBottom, backLeftTop, backLeftBottom, sideU, sideV);
        AddQuad(baseMeshData, frontRightTop, backRightTop, frontRightBottom, backRightBottom, sideU, sideV);
        AddQuad(baseMeshData, frontLeftTop, frontRightTop, frontLeftBottom, frontRightBottom, sideU, sideV);
        AddQuad(baseMeshData, backLeftTop, backLeftBottom, backRightTop, backRightBottom, sideU, sideV);
        AddQuad(baseMeshData, frontLeftBottom, frontRightBottom, backLeftBottom, backRightBottom, bottomU, bottomV);
    }

    private void AddRails(bool southOpen, bool northOpen, bool eastOpen, bool westOpen)
    {
        float halfWidth = _trackConstraintsData.TrackWidth / 2f;
        float railWidth = Mathf.Min(_trackConstraintsData.RailWidth, _trackConstraintsData.TrackWidth / 2f);
        float minX = -halfWidth;
        float maxX = halfWidth;
        float minZ = 0f;
        float maxZ = _trackConstraintsData.TrackWidth;

        AddClosedRail(southOpen, new Vector3(minX + railWidth, 0f, minZ), new Vector3(maxX - railWidth, 0f, minZ), Vector3.forward);
        AddClosedRail(northOpen, new Vector3(maxX - railWidth, 0f, maxZ), new Vector3(minX + railWidth, 0f, maxZ), Vector3.back);
        AddClosedRail(eastOpen, new Vector3(maxX, 0f, minZ + railWidth), new Vector3(maxX, 0f, maxZ - railWidth), Vector3.left);
        AddClosedRail(westOpen, new Vector3(minX, 0f, maxZ - railWidth), new Vector3(minX, 0f, minZ + railWidth), Vector3.right);

        AddRailCorner(new Vector3(minX, 0f, minZ), Vector3.right, Vector3.forward, westOpen, southOpen);
        AddRailCorner(new Vector3(maxX, 0f, minZ), Vector3.left, Vector3.forward, eastOpen, southOpen);
        AddRailCorner(new Vector3(minX, 0f, maxZ), Vector3.right, Vector3.back, westOpen, northOpen);
        AddRailCorner(new Vector3(maxX, 0f, maxZ), Vector3.left, Vector3.back, eastOpen, northOpen);
    }

    private void AddClosedRail(bool isOpen, Vector3 start, Vector3 end, Vector3 inward)
    {
        if (isOpen)
            return;

        AddRailStrip(start, end, inward, false, false);
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

        GetRailProfileOffsets(out float outerOffset, out float innerOffset);
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

}
