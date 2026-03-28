using UnityEngine;
using UnityEditor;

public class GameSpeedUp : MonoBehaviour
{
    // hit shift + z to toggle between 1x, 2x, 4x, and 6x battle speed
    // mainly for battle but can be used in overworld as well
    
    [MenuItem("Tools/Shortcuts/Toggle Game Speed #z")]
    private static void ToggleGameSpeed()
    {
        float currentSpeed = Time.timeScale;
        float newSpeed = (currentSpeed == 1f) ? 2f : (currentSpeed == 2f) ? 4f : (currentSpeed == 4f) ? 6f : 1f;
        Time.timeScale = newSpeed;
        Debug.Log($"[GameSpeedUp] Game speed toggled to {newSpeed}x");
    }

    [MenuItem("Tools/Shortcuts/Reset Game Speed %#z")]
    private static void ResetGameSpeed()
    {
        Time.timeScale = 1f;
        Debug.Log($"[GameSpeedUp] Game speed reset to 1x");
    }
}
