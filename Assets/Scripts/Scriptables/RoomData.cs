using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Scriptable Objects/Rooms/NewRoom", fileName = "NewRoom")]
public class RoomData : ScriptableObject
{
    [Header("Room Info")]
    public RoomID roomID;
    public bool isOverworldScene = true;
    
    [Header("Audio Settings")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 1f;
    
    [Header("Room Exits")]
    public RoomExits[] exits;

    [Header("Enemy Spawning per Room")]
    public RoomEnemySpawning enemySpawning;

#region Enums
    public enum RoomID
    {
        BattleRoom_00,
        Entrance153_01,
        Railroad154_02,
        Bookcase155_03
    }

    public enum SpawnPointID
    {
        North,
        South,
        East,
        West,
        SpiderWeb,
        None
    }
  #endregion
}


#region RoomExits
[System.Serializable]
public class RoomExits
{
    public RoomData.RoomID exitingTo;
    public RoomData.SpawnPointID spawnPointID;
    public RoomData targetRoom;
}
#endregion


#region RoomEnemySpawning
[System.Serializable]
public class RoomEnemySpawning
{
  public List<EnemyOverworldData> enemiesToSpawn;
  public int maxOverallSpawn;
  public int minOverallSpawn;
}
#endregion

