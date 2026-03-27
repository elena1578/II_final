using UnityEngine;


public class PathNode
{
    public Vector2Int gridPos;
    public bool walkable;
    public int gCost;  // distance from start node
    public int hCost;  // heuristic distance from end node
    public int fCost => gCost + hCost;
    public PathNode parent;

    public PathNode(Vector2Int pos, bool walkable)
    {
        gridPos = pos;
        this.walkable = walkable;
    }
}