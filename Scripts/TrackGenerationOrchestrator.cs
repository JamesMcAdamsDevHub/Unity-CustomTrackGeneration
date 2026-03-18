using UnityEngine;

public abstract class TrackGenerationOrchestrator : MonoBehaviour
{
#if UNITY_EDITOR
    public abstract void GenerateTrack();
    public virtual void RefreshFromConfig() { }
#endif
}