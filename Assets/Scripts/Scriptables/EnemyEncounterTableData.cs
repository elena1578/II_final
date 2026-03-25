using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyEncounterTable", menuName = "Scriptable Objects/Enemies/EnemyEncounterTable")]
public class EnemyEncounterTableData : ScriptableObject
{
    [System.Serializable]
    public class EncounterEntry
    {
        [Tooltip("Higher weight = more common encounter (1-100)")] [Range(1, 100)] public int weight = 1;
        [Tooltip("Enemies that appear in this encounter")] public List<EnemyBattleData> enemies;
    }

    [Header("Possible Encounters")]
    public List<EncounterEntry> encounters;
}