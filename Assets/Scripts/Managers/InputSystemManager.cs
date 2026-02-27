using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoBehaviour
{
    public static InputSystemManager instance;
    public InputActionAsset actions;
    // private string currentMap = "Overworld";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    // private void Update()
    // {
    //     Debug.Log("Current active action map: " + currentMap);
    // }

    public void SwapToBattleMap()
    {
        actions.FindActionMap("Overworld").Disable();
        actions.FindActionMap("Battle").Enable();
        // currentMap = "Battle";
    }

    public void SwapToOverworldMap()
    {
        actions.FindActionMap("Battle").Disable();
        actions.FindActionMap("Overworld").Enable();
        // currentMap = "Overworld";
    }
}
