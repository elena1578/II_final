using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class OverworldPathfinder : MonoBehaviour
{
    public static OverworldPathfinder instance;
    public LayerMask collisionMask;
    public float gridSize = 0.32f;  // use from GridMovementController to ensure pathfinding grid matches movement grid
    public Vector3 gridOffset = new Vector3(0.23f, -2.75f, 0f);

    private void Awake()
    {
        instance = this;
    }

    public bool IsWalkable(Vector3 worldPos)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        Vector3 snapped = GridToWorld(grid);

        float checkSize = gridSize * 0.6f;

        return !Physics2D.OverlapBox(
            snapped,  // center of box is snapped position (aligned to grid)
            new Vector2(checkSize, checkSize),
            0,
            collisionMask
        );
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - gridOffset.x) / gridSize);
        int y = Mathf.RoundToInt((worldPos.y - gridOffset.y) / gridSize);

        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int grid)
    {
        float x = grid.x * gridSize + gridOffset.x;
        float y = grid.y * gridSize + gridOffset.y;

        return new Vector3(x, y, 0f);
    }


    #region Helpers
    /// <summary>
    /// A* pathfinding used by overworld enemies to navigate toward the player.
    /// converts world positions to grid coords, searches for a valid path avoiding walls,
    /// and returns a list of world positions for the enemy to follow.
    /// 
    /// if no path is found (completely blocked), returns null
    /// </summary>
    public List<Vector3> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        // convert world positions -> grid coords so A* works on established grid
        Vector2Int start = WorldToGrid(startWorld);
        Vector2Int target = WorldToGrid(targetWorld);

        // open = unchecked nodes that need to be evaluated
        // closed = nodes that have already been evaluated/don't need checking again
        List<PathNode> open = new();
        HashSet<Vector2Int> closed = new();

        // create starting node & add to open list
        PathNode startNode = new PathNode(start, true);
        open.Add(startNode);

        // run A* search until path is found or no more nodes to check
        while (open.Count > 0)
        {
            PathNode current = open[0];

            // find node in open list w/ lowest total cost (fCost)
            // fCost = gCost (distance from start) + hCost (estimated distance to target)
            foreach (var node in open)
                if (node.fCost < current.fCost)
                    current = node;

            // move current node from open list -> closed list
            open.Remove(current);
            closed.Add(current.gridPos);

            // if target tile is reached, reconstruct path via following parent nodes back to start
            if (current.gridPos == target)
                return RetracePath(current);

            // check four neighboring tiles
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = current.gridPos + dir;

                if (closed.Contains(neighborPos))
                    continue;

                Vector3 world = GridToWorld(neighborPos);

                if (!IsWalkable(world))
                    continue;

                PathNode neighbor = new PathNode(neighborPos, true);

                neighbor.gCost = current.gCost + 10;  // gCost = movement cost from start -> this node
                neighbor.hCost =  
                    Mathf.Abs(neighborPos.x - target.x) +
                    Mathf.Abs(neighborPos.y - target.y);  // hCost = estimated cost from this node -> target

                neighbor.parent = current;  // remember which node led to this one (used when reconstructing path)

                // if neighbor is already in open list w/ lower fCost, skip adding it again
                bool alreadyOpen = open.Exists(n => n.gridPos == neighborPos);
                if (alreadyOpen)
                    continue;

                open.Add(neighbor);  // add neighbor to open list so it can be evaluated later
            }
        }
        return null;
    }

    private List<Vector3> RetracePath(PathNode endNode)
    {
        List<Vector3> path = new();

        PathNode current = endNode;

        while (current != null)
        {
            path.Add(GridToWorld(current.gridPos));
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        const int radius = 40;

        Vector3 origin = Vector3.zero;

        origin.x = Mathf.Round((origin.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
        origin.y = Mathf.Round((origin.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3 cell = new Vector3(
                    origin.x + x * gridSize,
                    origin.y + y * gridSize,
                    0
                );

                bool walkable = IsWalkable(cell);

                Gizmos.color = walkable ? new Color(0,1,0,0.25f) : new Color(1,0,0,0.35f);
                Gizmos.DrawCube(cell, Vector3.one * gridSize * 0.9f);
            }
        }
    }
#endif
    #endregion
}