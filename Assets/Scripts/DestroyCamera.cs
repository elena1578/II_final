using UnityEngine;


/// <summary>
/// unity build was having conflicts w/ overworldcamerafollow w/ this camera still existing and/or
/// follow one not initializing properly, so destroying it after the new scene loads fixes it
/// </summary>
public class DestroyCamera : MonoBehaviour
{
    private void OnSceneLoaded() => CheckForOtherCameras();

    private void CheckForOtherCameras()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (cameras.Length > 1)
        {
            Debug.LogWarning("[Destroy] Multiple cameras found. Destroying this camera to prevent conflicts");
            Destroy(gameObject);
        }
    }
}
