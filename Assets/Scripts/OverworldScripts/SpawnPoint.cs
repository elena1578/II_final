using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("The SpawnPointID should match the location where this object is placed (e.g., north exit = North)")] 
    public RoomData.SpawnPointID spawnPointID;
}
