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

    private void OnSceneLoaded() => ContextualizeScene();
    private void Awake()
    {
        instance = this;    

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        ContextualizeScene();
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
                    
                    Debug.Log($"Spawned {selectedData.name} at {pos}");
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

    private bool TryGetValidPosition(EnemyOverworldSpawnArea area, out Vector3 position)
    {
        int maxAttempts = 15;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidate = area.GetRandomPoint();
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.4f, collisionLayer);

            if (hit == null)
            {
                position = candidate;
                return true;
            }
        }

        position = Vector3.zero;
        return false;
    }
    #endregion
}
    
