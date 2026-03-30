using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class OverworldAStarPathfinder : MonoBehaviour
{
    public static OverworldAStarPathfinder instance;
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
            snapped,  // center of box (on grid) = snapped
            new Vector2(checkSize, checkSize),
            0,
            collisionMask
        );
    }

    /// <summary>
    /// same as GridMovementController's WorldToGrid
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - gridOffset.x) / gridSize);
        int y = Mathf.RoundToInt((worldPos.y - gridOffset.y) / gridSize);

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// same as GridMovementController's GridToWorld
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
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
    /// if no path is found (i.e., completely blocked), returns null
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

            // find node in open list w/ lowest total cost
            foreach (var node in open)
                if (node.totalCost < current.totalCost)
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

                // skip neighbor if already in closed list 
                if (closed.Contains(neighborPos))
                    continue;

                Vector3 world = GridToWorld(neighborPos);

                if (!IsWalkable(world))
                    continue;

                PathNode neighbor = new PathNode(neighborPos, true);

                // uniform cost for moving to any adjacent tile (no diagonals (hence why manhattan distance is used)), 
                // so just add 10 to current node's distance from start
                neighbor.distanceFromStart = current.distanceFromStart + 10; 
                neighbor.estimatedDistanceToGoal = Mathf.Abs(neighborPos.x - target.x) + Mathf.Abs(neighborPos.y - target.y); 

                neighbor.parent = current;  // remember which node led to this one (used when reconstructing path)

                // if neighbor is already in open list w/ lower total cost, skip adding
                bool alreadyOpen = open.Exists(n => n.gridPos == neighborPos);
                if (alreadyOpen)
                    continue;

                open.Add(neighbor); 
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

    /// <summary>
    /// directions for checking neighboring tiles during A* search
    /// </summary>
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
        float size = gridSize;
        const int radius = 40;  // how many cells to draw in each direction from current position
        Vector3 origin = Vector3.zero; 

        origin.x = Mathf.Round((origin.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
        origin.y = Mathf.Round((origin.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // cal center of each cell based on grid size & offset
                Vector3 cellCenter = new Vector3(
                    origin.x + x * gridSize,
                    origin.y + y * gridSize,
                    0
                );

                bool walkable = IsWalkable(cellCenter);  // also show which cells are walkable vs. blocked by walls

                Gizmos.color = walkable ? new Color(0,1,0,0.25f) : new Color(1,0,0,0.35f);  // green for walkable, red for blocked
                Gizmos.DrawCube(cellCenter, Vector3.one * gridSize * 0.9f);  
            }
        }
    }
#endif
    #endregion
}