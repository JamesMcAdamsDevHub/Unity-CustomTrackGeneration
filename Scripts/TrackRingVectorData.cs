using UnityEngine;

public class TrackRingVectorData
{
    public Vector3 TrackWidthFromCenter;
    public Vector3 TrackHeight;
    public Vector3 RailWidthFromCenter;
    public Vector3 RailRidgeTotalHeight;
    public Vector3 RailInnerRidgeWidthFromCenter;
    public Vector3 RailOuterRidgeWidthFromCenter;

    public TrackRingVectorData(TrackConstraintsData trackConstraintsData, Vector3 forward, Vector3 up)
    {
        Vector3 right = Vector3.Cross(forward, up).normalized;
        TrackWidthFromCenter = right * (trackConstraintsData.TrackWidth / 2);
        TrackHeight = up * trackConstraintsData.TrackHeight;
        RailWidthFromCenter = TrackWidthFromCenter - (right * trackConstraintsData.RailWidth);
        RailRidgeTotalHeight = TrackHeight + (up * trackConstraintsData.RailRidgeHeight);

        float railInnerRidgeOffset = trackConstraintsData.useSplitRidge
            ? trackConstraintsData.RailWidth / 2f - trackConstraintsData.RailWidth * trackConstraintsData.RailRidgePosition / 2f
            : trackConstraintsData.RailWidth * trackConstraintsData.RailRidgePosition;

        float railOuterRidgeOffset = trackConstraintsData.useSplitRidge
            ? trackConstraintsData.RailWidth / 2f + trackConstraintsData.RailWidth * trackConstraintsData.RailRidgePosition / 2f
            : trackConstraintsData.RailWidth * trackConstraintsData.RailRidgePosition;

        RailInnerRidgeWidthFromCenter = RailWidthFromCenter + (right * railInnerRidgeOffset);
        RailOuterRidgeWidthFromCenter = RailWidthFromCenter + (right * railOuterRidgeOffset);

    }
}
