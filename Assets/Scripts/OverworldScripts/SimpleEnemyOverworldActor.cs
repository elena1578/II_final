using UnityEngine;


/// <summary>
/// simplified version of EnemyOverworldActor (i.e., no movement or alert behavior).
/// mainly used for stationary boss enemies (i.e. king crawler) & testing
/// </summary>
public class SimpleEnemyOverworldActor : MonoBehaviour
{
    public EnemyOverworldData data;

    private Animator animator;
    private SpriteRenderer sr;
    private Transform playerTransform;
    private bool enteringBattle = false;
    private Camera cam;

    public void InitializeData(EnemyOverworldData enemyData)
    {
        data = enemyData;
        
        if (data != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null && data.sprite != null)
                sr.sprite = data.sprite;

            if (animator != null && data.animatorController != null)
                animator.runtimeAnimatorController = data.animatorController;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player") && !enteringBattle)
        {           
            // destroy other enemies to halt movement & prevent multiple battle triggers
            // if a save/load system is implemented, this would instead be saving what enemies there are and
            // respawning them after battle rather vs. destroying them

            EnemyOverworldActor[] otherEnemies = FindObjectsByType<EnemyOverworldActor>(FindObjectsSortMode.None);
            foreach (EnemyOverworldActor enemy in otherEnemies)            
            {
                if (enemy != this)
                    Destroy(enemy.gameObject);
            }

            enteringBattle = true;

            // disable player movement
            PlayerOverworldController player = other.collider.GetComponent<PlayerOverworldController>();
            player?.FreezeForBattle();

            // audio & effects
            AudioManager.instance?.PlaySFX(AudioManager.instance.battleStart, 0.5f);

            cam = Camera.main;
            if (cam != null)            
            {
                OverworldCameraScreenShake screenShake = cam.GetComponent<OverworldCameraScreenShake>();
                screenShake?.Shake();
            }

            Debug.Log("[EnemyOverworldActor] Beginning battle with " + data.name);
            BattleTransitionManager.instance.StartBattle(data);
        }
    }
}
