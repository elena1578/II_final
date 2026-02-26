using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


public class CheckEnemySpawning : MonoBehaviour
{
    // 2. gather the types of enemies assigned to each room and determine which enemies are not being spawned BUT have
    // existing data 
    
    // 1. get list of all existing enemy overworld data
    // 2. get list of all enemies assigned in RoomData
    // 3. compare the two lists and print out any enemies that are not assigned to a room to spawn but have data

    [MenuItem("Tools/Check Enemy Spawning")]
    private static void CheckSpawning()
    {
        List<EnemyOverworldData> allEnemyData = GetAllEnemyData();
        List<RoomData> allRoomData = GetAllRoomData();
        int totalEnemies = GetTotalNumberOfEnemies();

        foreach (EnemyOverworldData enemy in allEnemyData)
        {
            // go per the enemy vs. by the room
            // i.e., iterate through each enemy and check if it's in any room's spawn list
            // rather than going through each room and checking if the enemy is in it
            
            bool found = false;
            int counter = 0;

            foreach (EnemyOverworldData enemyData in allRoomData.SelectMany(room => room.enemySpawning.enemiesToSpawn))
            {
                counter++;

                if (enemyData == enemy)
                {
                    found = true;
                    // Debug.Log($"[CheckEnemySpawning] Enemy '{enemy.name}' is assigned to spawn in a room. Counter = {counter}");
                    if (counter == totalEnemies)
                    {
                        Debug.Log($"[CheckEnemySpawning] All enemies are assigned to spawn in rooms.");
                        return;
                    }
                    break;
                }
            }
            if (!found)
                Debug.LogWarning($"[CheckEnemySpawning] Enemy '{enemy.name}' has data but is not assigned to spawn in any room.");
        }
    }

    private static List<EnemyOverworldData> GetAllEnemyData()
    {
        string[] guids = AssetDatabase.FindAssets("t:EnemyOverworldData");
        List<EnemyOverworldData> dataList = new List<EnemyOverworldData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyOverworldData data = AssetDatabase.LoadAssetAtPath<EnemyOverworldData>(path);
            if (data != null)
                dataList.Add(data);
        }
        return dataList;
    }

    private static int GetTotalNumberOfEnemies()
    {
        List<EnemyOverworldData> allEnemyData = GetAllEnemyData();  
        int total = allEnemyData.Count;
        return total;
    }

    private static List<RoomData> GetAllRoomData()
    {
        string[] guids = AssetDatabase.FindAssets("t:RoomData");
        List<RoomData> dataList = new List<RoomData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RoomData data = AssetDatabase.LoadAssetAtPath<RoomData>(path);
            if (data != null)
                dataList.Add(data);
        }
        return dataList;
    }
}
