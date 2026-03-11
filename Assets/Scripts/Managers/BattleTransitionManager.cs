using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;


public class BattleTransitionManager : MonoBehaviour
{
    public static BattleTransitionManager instance;
    [HideInInspector] public RoomData.RoomID storedRoomID;
    [HideInInspector] public Vector3 storedPlayerPosition;
    [HideInInspector] public EnemyOverworldData currentEnemy;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartBattle(EnemyOverworldData enemy)
    {
        // store current room and player position before transitioning to battle
        RoomData.RoomID currentRoomID = RoomManager.GetRoomFromActiveScene().roomID;

        PlayerOverworldController playerController = FindFirstObjectByType<PlayerOverworldController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerOverworldController not found in the scene!");
            return;
        }

        // store player position and disable movement
        Vector3 playerPosition = playerController != null ? playerController.transform.position : Vector3.zero;
        playerController.DisablePlayerMovement();

        currentEnemy = enemy;
        if (currentEnemy == null)
        {
            Debug.LogError("No enemy data provided for battle transition!");
            return;
        }

        StoreCurrentData(currentRoomID, playerPosition, currentEnemy);

        // transition to battle scene
        RoomChangeManager.instance.InitializeAndGoToRoom(currentRoomID, RoomData.RoomID.BattleRoom_00);
    }

    public void ReturnToOverworld()
    {
        // probably need to add a specific from-battle method
        // either here or roomchangemanager to account going to a vector3 vs. a spawn point
        // think i did ^^^ but keeping here just in case it blows up 

// if starting from battle room for debugging
#if UNITY_EDITOR
        if (storedRoomID == RoomData.RoomID.BattleRoom_00)
        {
            storedRoomID = RoomData.RoomID.Entrance153_01;  // default room
            RoomChangeManager.instance.InitializeAndReturnFromBattle(RoomManager.GetRoom(storedRoomID), new Vector3(4.701f, -1.785f, 0f));
            ClearCurrentData();
        }
#endif

        RoomChangeManager.instance.InitializeAndReturnFromBattle(RoomManager.GetRoom(storedRoomID), storedPlayerPosition);
        ClearCurrentData();
    }

    public void ReturnToTitleScreen()
    {
        StartCoroutine(LoadTitleScreenRoutine());
        ClearCurrentData();
    }

    private IEnumerator LoadTitleScreenRoutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TitleScreen_99");

        while (!asyncLoad.isDone)
            yield return null;
    }

    public void StoreCurrentData(RoomData.RoomID roomID, Vector3 playerPosition, EnemyOverworldData enemy)
    {
        storedRoomID = roomID;
        storedPlayerPosition = playerPosition;
        currentEnemy = enemy;
    }

    private void ClearCurrentData()
    {
        storedRoomID = RoomData.RoomID.Entrance153_01;  // default room
        storedPlayerPosition = Vector3.zero;
        currentEnemy = null;
    }
}
