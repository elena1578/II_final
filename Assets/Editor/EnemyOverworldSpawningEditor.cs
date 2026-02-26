using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyOverworldSpawner))]
public class EnemyOverworldSpawnerEditor : Editor
{
    // 1. respawn enemies to check types + bounds (min/max) via a button (done)
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EnemyOverworldSpawner spawner = (EnemyOverworldSpawner)target;

        if (GUILayout.Button("Spawn Enemies"))
        {
            spawner.RespawnEnemies();
            Debug.Log("[EnemyOverworldSpawnerEditor] Respawned enemies");
        }
    }
}
