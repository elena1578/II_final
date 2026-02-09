using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoBehaviour
{
    public static InputSystemManager instance;
    public InputActionAsset actions;

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

    public void SwapToBattleMap()
    {
        actions.FindActionMap("Overworld").Disable();
        actions.FindActionMap("Battle").Enable();
    }

    public void SwapToOverworldMap()
    {
        actions.FindActionMap("Battle").Disable();
        actions.FindActionMap("Overworld").Enable();
    }
}
