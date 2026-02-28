using UnityEngine;
using UnityEditor;


// only works in play mode
public class GetCurrentRoom : MonoBehaviour
{
    [MenuItem("Tools/Get Current Room")]
    private static void GetCurrentRoomFromScene()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Get Current Room can only be used in play mode.");
            return;
        }
        Debug.Log($"[Get Current Room] Current room: {RoomManager.GetRoomFromActiveScene()?.roomID}");
    }
}
