using UnityEngine;

[CreateAssetMenu(fileName = "EnemyOverworldData", menuName = "Scriptable Objects/Enemies/EnemyOverworldData")]
public class EnemyOverworldData : ScriptableObject
{
    [Header("Definition")]
    public EnemyBattleData correspondingBattleData;
    public EnemyEncounterTableData encounterTable;  // if null, correspondingBattleData will be used for encounters instead
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
    [Range(0f, 1f)] public float spawnChance = 0.5f;

    [Header("Behavior Settings")]
    [Range(0f, 1.5f)] public float moveSpeed = 1f;
    [Range(0f, 1f)] public float chanceToMove = 1f;
    public float alertRadius = 5f;
}
