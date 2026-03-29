using UnityEngine;

public class LocalPointData
{
    public Vector3 localPosition, localForward, localUp;

    public LocalPointData() 
    {
        localPosition = Vector3.zero;
        localForward = Vector3.forward;
        localUp = Vector3.up;
    }

    public LocalPointData(Vector3 localPosition, Vector3 localForward, Vector3 localUp) 
    { 
        this.localPosition = localPosition;
        this.localForward = localForward;
        this.localUp = localUp;
    }
}
