using UnityEngine;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
public class OverworldDebugTool : MonoBehaviour
{
    private InputAction currentRoomDebug;


    private void OnEnable()
    {
        currentRoomDebug = InputSystemManager.instance.actions["CurrentRoomDebugPrint"];
        currentRoomDebug.Enable();
    }

    private void OnDisable()
    {
        currentRoomDebug?.Disable();
    }

    private void Update()
    {
        if (!currentRoomDebug.WasPressedThisFrame())
            return;

        Debug.Log($"[OverworldDebugTool] Current Room: {RoomManager.GetRoomFromActiveScene()?.roomID}");
    }
}
#endif


