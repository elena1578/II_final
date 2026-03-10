using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private RoomCollectionDatabase roomCollection;
    private float duration;
    public static event Action OnStartingQuitHold, OnCancelingQuitHold;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        if (roomCollection != null)
            RoomManager.Initialize(roomCollection);

        // disable logging in builds to improve performance
        if (!Application.isEditor)
            Debug.unityLogger.logEnabled = false;
    }

    private void OnEnable()
    {
        var quitAction = new InputAction(binding: "<Keyboard>/escape", interactions: "hold(duration=2)");

        quitAction.started += ctx => OnStartingQuitHold();
        quitAction.performed += ctx => QuitGameSession();
        quitAction.canceled += ctx => OnCancelingQuitHold();

        quitAction.Enable();
        Debug.Log("[GameManager] Can now quit game session by holding Escape for 2 seconds");
    }

    private void OnDisable()
    {
        var quitAction = new InputAction(binding: "<Keyboard>/escape", interactions: "hold(duration=2)");
        quitAction.Disable();
    }

    /// <summary>
    /// quits game. if editor, stops play mode. if built, quits application.
    /// will later be used for quit button on title screen & pause menu
    /// </summary>
    public void QuitGameSession()
    {
        Debug.Log("[GameManager] Quitting game...");

#if UNITY_EDITOR
      // stop play mode if in editor
      UnityEditor.EditorApplication.isPlaying = false;
#else
        // quit application if built
        Application.Quit();
#endif
    }
}


#region RoomManager
[System.Serializable]
public static class RoomManager
{
    public static Dictionary<RoomData.RoomID, RoomData> RoomDictionary;

    /// <summary>
    /// initializes RoomDictionary from RoomCollectionDatabase scriptable.
    /// called once on game start
    /// </summary>
    /// <param name="collection"></param>
    public static void Initialize(RoomCollectionDatabase collection)
    {
        RoomDictionary = new Dictionary<RoomData.RoomID, RoomData>();

        foreach (var room in collection.rooms)
        {
            if (!RoomDictionary.ContainsKey(room.roomID))
                RoomDictionary.Add(room.roomID, room);
            else
                Debug.LogError($"Duplicate RoomID detected: {room.roomID}");
        }

        Debug.Log($"[RoomManager] Initialized with {RoomDictionary.Count} rooms.");
    }

    /// <summary>
    /// gets RoomData by RoomID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static RoomData GetRoom(RoomData.RoomID id)
    {
        if (RoomDictionary == null)
        {
            Debug.LogError("RoomDictionary null");
            return null;
        }

        if (RoomDictionary.TryGetValue(id, out var room))
        {
            return room;
        }

        Debug.LogError($"[RoomManager] RoomID {id} not found in RoomManager");
        return null;
    }

    /// <summary>
    /// finds the RoomData for the currently active scene.
    /// scene name must match RoomID enum value exactly
    /// </summary>
    public static RoomData GetRoomFromActiveScene()
    {
        if (RoomDictionary == null)
        {
            Debug.LogError("[RoomManager] RoomDictionary null");
            return null;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;

        if (System.Enum.TryParse(activeSceneName, out RoomData.RoomID roomID))
        {
            return GetRoom(roomID);
        }

        Debug.LogError($"[RoomManager] Active scene '{activeSceneName}' does not match any RoomID");
        return null;
    }
}
#endregion
