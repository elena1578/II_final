using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RoomCollectionDatabase", menuName = "Scriptable Objects/Rooms/RoomCollectionDatabase")]
public class RoomCollectionDatabase : ScriptableObject
{
    public List<RoomData> rooms;
}
