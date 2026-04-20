using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class RoomChangeManager : MonoBehaviour
{
    public static RoomChangeManager instance;
    private ScreenFade screenFade;
    private PlayerOverworldController player;


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
    
    private void Start()
    {
        RoomData startingRoom = RoomManager.GetRoomFromActiveScene();
        if (startingRoom != null)
        {
            if (startingRoom.music != null)
                    MusicFadeInOut.instance.CheckMusic(startingRoom.music, startingRoom.musicVolume);
                else
                    MusicFadeInOut.instance.StopMusic();
        }
    }


    #region Standard
    /// <summary>
    /// Call to to transition from currentRoomID to exitingTo room
    /// </summary>
    /// <param name="currentRoomID"></param>
    /// <param name="exitingTo"></param>
    public void InitializeAndGoToRoom(RoomData.RoomID currentRoomID, RoomData.RoomID exitingTo)
    {
        RoomData currentRoom = RoomManager.GetRoom(currentRoomID);  // get ID from RoomManager in GameManager
        if (currentRoom == null)
        {
            Debug.LogError($"[RoomChangeManager] Couldn't find current room: {currentRoomID}");
            return;
        }

        // get exit
        RoomExits exit = Exit(currentRoom, exitingTo);
        if (exit == null)
        {
            Debug.LogError($"[RoomChangeManager] No exit found from {currentRoomID} to {exitingTo}");
            return;
        }

        // check if target room is null
        if (exit.targetRoom == null)
        {
            Debug.LogError($"[RoomChangeManager] Exit from {currentRoomID} to {exitingTo} has no target room assigned");
            return;
        }

        StartCoroutine(LeaveRoomRoutine(exit.targetRoom, exit.spawnPointID));
    }

    private RoomExits Exit(RoomData room, RoomData.RoomID exitingTo)
    {
        foreach (var exit in room.exits)
        {
            if (exit.exitingTo == exitingTo)
                return exit;
        }
        return null;
    }

    private IEnumerator LeaveRoomRoutine(RoomData targetRoom, RoomData.SpawnPointID spawnPointID)
    {
        screenFade = FindFirstObjectByType<ScreenFade>();
        if (screenFade == null)
        {
            Debug.LogWarning("screenFade missing");
            yield break;
        }

        // disable player movement
        player = FindFirstObjectByType<PlayerOverworldController>();
        if (player != null)
            player.DisablePlayerMovement();

        // fade out music
        if (MusicFadeInOut.instance != null)
            MusicFadeInOut.instance.PreTransitionCheckMusic(targetRoom.music);

        // fade to black
        screenFade.StartCoroutine(screenFade.FadeIn());
        yield return new WaitForSeconds(screenFade.fadeDuration);

        // load new room
        yield return StartCoroutine(EnterRoomRoutine(targetRoom, spawnPointID));
    }

    private IEnumerator EnterRoomRoutine(RoomData targetRoom, RoomData.SpawnPointID spawnPointID)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetRoom.roomID.ToString());  // e.g., "Entrance153_01"

        // show loading screen if load takes awhile 
        float timer = 0f;
        bool loadingShown = false;
        float loadingDelay = 0.4f;

        while (!asyncLoad.isDone)
        {
            timer += Time.deltaTime;

            if (!loadingShown && timer >= loadingDelay)
            {
                LoadingScreen.instance?.Show();
                loadingShown = true;
            }

            yield return null;
        }
        LoadingScreen.instance?.Hide();

        // place player immediately after scene loads prior to fade out
        if (targetRoom.isOverworldScene)
        {
            GameObject playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                Rigidbody2D rb = playerGO.GetComponent<Rigidbody2D>();
                PlayerOverworldController controller =
                    playerGO.GetComponent<PlayerOverworldController>();

                if (rb != null)
                    rb.bodyType = RigidbodyType2D.Kinematic;

                SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
                foreach (var sp in spawnPoints)
                {
                    if (sp.spawnPointID == spawnPointID)
                    {
                        // match spawn point ID then snap to grid 
                        playerGO.transform.position = sp.transform.position;
                        controller.ForceSnapToGrid(sp.transform.position);
                        Debug.Log($"Player spawned at {spawnPointID} in {targetRoom.roomID}");

                        break;
                    }
                }

                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.linearVelocity = Vector2.zero;
                }

                if (controller != null)
                    controller.EnablePlayerMovement();
            }
            else
                Debug.Log("No player found in the scene after room change, skipping player placement");
        }

        // now fade out
        screenFade = FindFirstObjectByType<ScreenFade>();
        if (screenFade != null)
        {
            yield return StartCoroutine(FadeOutForNewRoom(targetRoom));
        }

        // battle case: if music is assigned to detected enemy battle data,
        // use that instead of what's assigned to RoomData
        if (BattleTransitionManager.instance != null && BattleTransitionManager.instance.currentEnemy != null)
        {
            EnemyOverworldData enemyData = BattleTransitionManager.instance.currentEnemy;

            if (enemyData.correspondingBattleData != null && enemyData.correspondingBattleData.music != null)
            {
                MusicFadeInOut.instance.CheckMusic(
                    enemyData.correspondingBattleData.music,
                    enemyData.correspondingBattleData.musicVolume
                );
                yield break;
            }
        }

        // standard case: use RoomData music
        if (targetRoom.music != null)
            MusicFadeInOut.instance.CheckMusic(targetRoom.music, targetRoom.musicVolume);
        else
            MusicFadeInOut.instance.StopMusic();
    }
    #endregion


    #region Battle Case
    public void InitializeAndReturnFromBattle(RoomData targetRoom, Vector3 playerPosition)
        => StartCoroutine(LeaveBattleRoutine(targetRoom, playerPosition));

    private IEnumerator LeaveBattleRoutine(RoomData targetRoom, Vector3 playerPosition)
    {
        screenFade = FindFirstObjectByType<ScreenFade>();
        if (screenFade == null)
        {
            Debug.LogWarning("ScreenFade missing");
            yield break;
        }

        // fade out music
        if (MusicFadeInOut.instance != null)
            MusicFadeInOut.instance.PreTransitionCheckMusic(targetRoom.music);

        // fade to black
        screenFade.StartCoroutine(screenFade.FadeIn());
        yield return new WaitForSeconds(screenFade.fadeDuration);

        // load new room
        yield return StartCoroutine(ReturnToRoomRoutine(targetRoom, playerPosition));
    }

    private IEnumerator ReturnToRoomRoutine(RoomData targetRoom, Vector3 playerPosition)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetRoom.roomID.ToString());  // e.g., "Entrance153_01"

        // show loading screen if load takes awhile 
        float timer = 0f;
        bool loadingShown = false;
        float loadingDelay = 0.4f;

        while (!asyncLoad.isDone)
        {
            timer += Time.deltaTime;

            if (!loadingShown && timer >= loadingDelay)
            {
                LoadingScreen.instance?.Show();
                loadingShown = true;
            }

            yield return null;
        }
        LoadingScreen.instance?.Hide();

        // freeze enemies for a few seconds after returning from battle to prevent immediate re-entering
        EnemyOverworldSpawner spawner = FindFirstObjectByType<EnemyOverworldSpawner>();
        if (spawner != null)
        {
            EnemyOverworldActor[] enemies = FindObjectsByType<EnemyOverworldActor>(FindObjectsSortMode.None);

            if (enemies.Length == 0)
                Debug.LogWarning("[RoomChangeManager] No enemies found to freeze after returning from battle");
            else
            {
                foreach (var enemy in enemies)
                {
                    float duration = 5f;
                    Debug.Log($"[RoomChangeManager] Freezing {enemy.data.correspondingBattleData} for {duration} seconds after battle transition");
                    enemy.FreezeForDuration(duration);
                }
            }
        }
        else
            Debug.LogWarning("[RoomChangeManager] No EnemyOverworldSpawner found, skipping enemy freeze after returning from battle");

        // place player immediately after scene loads prior to fade out
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            Rigidbody2D rb = playerGO.GetComponent<Rigidbody2D>();
            PlayerOverworldController controller =
                playerGO.GetComponent<PlayerOverworldController>();

            if (rb != null)
                rb.bodyType = RigidbodyType2D.Kinematic;

            if (controller != null)
            {
                Debug.Log("[RoomChangeManager] Stored player position from BattleTransitionManager: " + playerPosition);
                controller.StartCoroutine(controller.ForceSnapToGridNextFrame(controller, playerPosition));
            }
            else
                playerGO.transform.position = playerPosition;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
            }

            if (controller != null)
            {
                InputSystemManager.instance.SwapToOverworldMap();  // ensure correct input map
                controller.EnablePlayerMovement();
            }
            else
                Debug.Log("No player found in the scene after battle transition, skipping player placement");
        }

        // now fade out
        screenFade = FindFirstObjectByType<ScreenFade>();
        if (screenFade != null)
        {
            yield return StartCoroutine(FadeOutForNewRoom(targetRoom));
        }

        if (targetRoom.music != null)
            MusicFadeInOut.instance.CheckMusic(targetRoom.music, targetRoom.musicVolume);
        else
            MusicFadeInOut.instance.StopMusic();
    }
    #endregion

    private IEnumerator FadeOutForNewRoom(RoomData targetRoom)
    {
        if (screenFade == null)
            yield break;

        screenFade.fadeCanvasGroup.alpha = 1;
        yield return screenFade.StartCoroutine(screenFade.FadeOut());
    }
}

