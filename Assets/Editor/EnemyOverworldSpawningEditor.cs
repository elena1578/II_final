using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyOverworldSpawner))]
public class EnemyOverworldSpawnerEditor : Editor
{
    // this should be used to:
    // 1. respawn enemies to check types + bounds (min/max) via a button
    // 2. gather the types of enemies assigned to each room and determine which enemies are not being spawned BUT have
        // existing data
    
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
