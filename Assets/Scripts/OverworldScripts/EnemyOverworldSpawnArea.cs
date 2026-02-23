using UnityEngine;


public class EnemyOverworldSpawnArea : MonoBehaviour
{
    [Header("Spawn Area Size")]
    public Vector2 size = new Vector2(10f, 6f);
    public Vector2 offset;

    [Header("Spawn Limits")]
    public int minSpawn = 1;
    public int maxSpawn = 3;

    private Vector3 AreaCenter
    {
        get
        {
            return transform.position + new Vector3(offset.x, offset.y, 0f);
        }
    }

    public Vector3 GetRandomPoint()
    {
        Vector3 center = AreaCenter;

        return new Vector3(
            Random.Range(center.x - size.x / 2f, center.x + size.x / 2f),
            Random.Range(center.y - size.y / 2f, center.y + size.y / 2f),
            0f
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(AreaCenter, size);
    }
}