using UnityEngine;

[CreateAssetMenu(fileName = "EnemyOverworldData", menuName = "Scriptable Objects/Enemies/EnemyOverworldData")]
public class EnemyOverworldData : ScriptableObject
{
    [Header("Definition")]
    public GameObject enemyPrefab;
    public EnemyBattleData correspondingBattleData;
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;

    [Header("Behavior Settings")]
    [Range(0f, 1.5f)] public float moveSpeed = 1f;
    [Range(0f, 1f)] public float chanceToMove = 1f;
    public float alertRadius = 5f;
}
