using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class EnemyOverworldSpawner : MonoBehaviour
{
    public static EnemyOverworldSpawner instance;
    public GameObject enemyActorPrefab;
    public LayerMask collisionLayer;
    
    // 1. call current room from RoomManager
    // 2. get enemy spawn data from RoomData
    // 3. spawn enemies based on that data
    // 4. check for viable spawn placement (no collisions, within room bounds, etc.)
    // 5. snap to grid once spawned (this will prob happen in the enemy actor scrcipt)

    private EnemyOverworldSpawnArea[] spawnAreas;
    private RoomData currentRoomData;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ContextualizeScene();
    }
    
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

    private void SpawnEnemies()
    {
        var spawnList = currentRoomData.enemySpawning.enemiesToSpawn;

        if (spawnList == null || spawnList.Count == 0)
        {
            Debug.Log("[EnemyOverworldSpawner] No enemies set to spawn for this room");
            return;
        }

        foreach (var area in spawnAreas)
        {
            int count = Random.Range(area.minSpawn, area.maxSpawn + 1);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (TryGetValidPosition(area, out pos))
                {
                    EnemyOverworldData selectedData = GetRandomEnemyData();
                    if (selectedData == null)
                        continue;

                    GameObject enemyGO = Instantiate(enemyActorPrefab, pos, Quaternion.identity);

                    // initialize enemy actor w/ the randomly selected data
                    EnemyOverworldActor actor = enemyGO.GetComponent<EnemyOverworldActor>();
                    actor.InitializeData(selectedData);

                    if (selectedData == null)
                    {
                        Debug.LogError($"[EnemyOverworldSpawner] Selected enemy data is null for {selectedData.name}");
                        continue;
                    }

                    actor.SetSpawnArea(area);  // set spawn area so enemy can't walk outside it 
                    
                    Debug.Log($"[EnemyOverworldSpawner] Spawned {selectedData.name} at {pos}");
                }
            }
        }
    }


    #region Scene Context
    private void ContextualizeScene()
    {
        FindCurrentSpawnAreas();
        GetCurrentRoomData();
    }

    private void FindCurrentSpawnAreas()
    {
        spawnAreas = FindObjectsByType<EnemyOverworldSpawnArea>(FindObjectsSortMode.None);

        if (spawnAreas.Length == 0)
        {
            Debug.LogWarning("No spawn areas found in scene.");
            return;
        }
    }

    private void GetCurrentRoomData()
    {
        currentRoomData = RoomManager.GetRoomFromActiveScene();

        if (currentRoomData == null)
        {
            Debug.LogError("Current room data is null!");
            return;
        }

        SpawnEnemies();
    }
    #endregion


    #region Helpers
    private EnemyOverworldData GetRandomEnemyData()
    {
        var list = currentRoomData.enemySpawning.enemiesToSpawn;

        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("[EnemyOverworldSpawner] Enemy spawn list is empty or null");
            return null;
        }

        int index = Random.Range(0, list.Count);
        return list[index];
    }

    /// <summary>
    /// need to check for collisions and pathfinding walkability to ensure enemies don't spawn in unreachable locations 
    /// or inside walls/objects
    /// </summary>
    /// <param name="area"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool TryGetValidPosition(EnemyOverworldSpawnArea area, out Vector3 position)
    {
        int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidate = area.GetRandomPoint();
            
            // use pathfinder's walkability check (which uses a slightly smaller collision box than the physical one) 
            // to weed out positions that are technically not colliding but would still be unreachable for enemies
            Vector3 snapped = OverworldAStarPathfinder.instance.GridToWorld(
                OverworldAStarPathfinder.instance.WorldToGrid(candidate)
            );

            if (!OverworldAStarPathfinder.instance.IsWalkable(snapped))
                continue;

            // if collision @ candidate position, try to find spawn pos again
            // check physical collision (walls/objects)
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.4f, collisionLayer);
            if (hit != null)
                continue;

            // check pathfinder walkability (since counts collision a bit differently)
            if (OverworldAStarPathfinder.instance != null &&
                !OverworldAStarPathfinder.instance.IsWalkable(candidate))
                continue;

            position = snapped;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    public void RespawnEnemies()
    {
        // destroy existing enemies
        EnemyOverworldActor[] enemies = FindObjectsByType<EnemyOverworldActor>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        // respawn new enemies
        SpawnEnemies();
    }

    public void FreezeAllEnemies(float duration)
    {
        EnemyOverworldActor[] enemies = FindObjectsByType<EnemyOverworldActor>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            enemy.FreezeForDuration(duration);
        }
    }
    
    public void UnfreezeAllEnemies()
    {
        EnemyOverworldActor[] enemies = FindObjectsByType<EnemyOverworldActor>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            enemy.Unfreeze();
        }
    }

    // same one as in GridMovementController (but that one is protected)
    private Vector3 SnapToGrid(Vector3 pos)
    {
        float gridSize = 0.32f;
        Vector3 gridOffset = new Vector3(0.23f, -2.75f, 0f);

        float x = Mathf.Round((pos.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
        float y = Mathf.Round((pos.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;

        return new Vector3(x, y, pos.z);
    }
    #endregion
}

    
