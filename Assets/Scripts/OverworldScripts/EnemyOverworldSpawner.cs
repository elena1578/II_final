using UnityEngine;
using System.Collections.Generic;


public class EnemyOverworldSpawner : MonoBehaviour
{
    [Header("Testing Vals")]
    public Vector3 roomCenter;
    public Vector3 roomSize;

    public GameObject enemyActorPrefab;
    private GridMovementController gridController;
    private LayerMask collisionLayer;
    
    // internal vars that will hold RoomData data
    public static EnemyOverworldSpawner instance;
    private RoomData currentRoomData;   
    private List<EnemyOverworldData> enemiesToSpawn;
    private int minOverallSpawn;
    private int maxOverallSpawn;
    
    // 1. call current room from RoomManager
    // 2. get enemy spawn data from RoomData
    // 3. spawn enemies based on that data
    // 4. check for viable spawn placement (no collisions, within room bounds, etc.)
    // 5. snap to grid once spawned (this will prob happen in the enemy actor scrcipt)

    private void Awake()
    {
        instance = this;

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        ClearData();
    }

    private void ClearData()
    {
        currentRoomData = null;
        enemiesToSpawn = null;
        minOverallSpawn = 0;
        maxOverallSpawn = 0;
    }

    /// <summary>
    /// match local vars w/ data from RoomData
    /// </summary>
    private void Start()
    {
        collisionLayer = LayerMask.GetMask("Collision");
        
        currentRoomData = RoomManager.GetRoomFromActiveScene();
        if (currentRoomData != null)
        {          
            enemiesToSpawn = currentRoomData.enemySpawning.enemiesToSpawn;
            minOverallSpawn = currentRoomData.enemySpawning.minOverallSpawn;
            maxOverallSpawn = currentRoomData.enemySpawning.maxOverallSpawn;

            SpawnEnemies();
        }
        else
            Debug.LogWarning("Current RoomData is null.");
    }

    private void SpawnEnemies()
    {
        if (enemiesToSpawn == null || enemiesToSpawn.Count == 0)
        {
            Debug.Log("This is probably the BattleRoom, skipping enemy spawn (otherwise enemiesToSpawn is null)");
            return;
        }

        int spawnCount = Random.Range(minOverallSpawn, maxOverallSpawn + 1);
        for (int i = 0; i < spawnCount; i++)
        {
            EnemyOverworldData enemyData = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Count)];
            Vector3 spawnPos = GetRandomValidSpawnPosition();
            GameObject enemyGO = Instantiate(enemyActorPrefab, spawnPos, Quaternion.identity);
        }
    }

    private Vector3 GetRandomValidSpawnPosition()
    {
        // int attempts = 0;
        // int maxAttempts = 30;
        return new Vector3(
            Random.Range(roomCenter.x - roomSize.x / 2, roomCenter.x + roomSize.x / 2),
            Random.Range(roomCenter.y - roomSize.y / 2, roomCenter.y + roomSize.y / 2),
            0
        );
    }   

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(roomCenter, roomSize);
    }
}
