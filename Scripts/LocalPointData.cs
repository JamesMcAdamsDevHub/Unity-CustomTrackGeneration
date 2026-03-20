using UnityEngine;

public class LocalPointData
{
    public Vector3 localPosition, localForward, localUp;

    public LocalPointData() { }

    public LocalPointData(Vector3 localPosition, Vector3 localForward, Vector3 localUp) 
    { 
        this.localPosition = localPosition;
        this.localForward = localForward;
        this.localUp = localUp;
    }
}
