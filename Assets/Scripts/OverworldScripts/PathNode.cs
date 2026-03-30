using UnityEngine;


public class PathNode
{
    public Vector2Int gridPos;
    public bool walkable;

    // ref: https://www.geeksforgeeks.org/artificial-intelligence/a-algorithm-and-its-heuristic-search-strategy-in-artificial-intelligence/
    public int distanceFromStart;
    public int estimatedDistanceToGoal;
    public int totalCost => distanceFromStart + estimatedDistanceToGoal;
    public PathNode parent;

    public PathNode(Vector2Int pos, bool walkable)
    {
        gridPos = pos;
        this.walkable = walkable;
    }
}