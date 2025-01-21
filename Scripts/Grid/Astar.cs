using UnityEngine;
using System.Collections.Generic;

public class Astar : MonoBehaviour
{
    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end, bool noFog = true)
    {
        List<Vector3Int> openSet = new();
        List<Vector3Int> closedSet = new();

        Dictionary<Vector3Int, Vector3Int> parents = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, float> costs = new Dictionary<Vector3Int, float>();

        GridManager gridManager = References.Instance.GridDataManager;

        List<Vector3Int> path = new List<Vector3Int>();

        if (!gridManager.TileData.tiles.ContainsKey(end)
            || (!gridManager.TileData.tiles[end].IsPassable && gridManager.TileData.tiles[end].WasSeen)
            || (noFog && !gridManager.TileData.tiles[end].IsPassable))
        {
            Debug.LogError("destination is not passable");
            return path;
        }

        openSet.Add(start);

        parents.Add(start, start);
        costs.Add(start, 0);


        Vector3Int current = openSet[0];
        while (openSet.Count > 0)
        {
            current = openSet[0];
            float currentCost = GetTileCost(current, end);
            foreach (var tile in openSet)
            {
                float tileCost = GetTileCost(tile, end);
                if (tileCost <= currentCost)
                {
                    current = tile;
                    currentCost = tileCost;
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == end)
            {
                break;
            }

            foreach (var tile in gridManager.GetNeighbours(current))
            {
                if ((!gridManager.TileData.tiles[tile].IsPassable && gridManager.TileData.tiles[tile].WasSeen) 
                    || closedSet.Contains(tile) 
                    || (noFog && !gridManager.TileData.tiles[tile].IsPassable))
                {
                    continue;
                }
                float newCost = costs[current] + GetTileCost(tile, end);
                if (!costs.ContainsKey(tile) || newCost < costs[tile])
                {
                    costs[tile] = newCost;
                    parents[tile] = current;
                    
                    if (!openSet.Contains(tile))
                        openSet.Add(tile);
                }
            }
        }


        while (current != start)
        {
            path.Add(current);
            current = parents[current];
        }
        path.Reverse();

        return path;
    }

    private float GetTileCost(Vector3Int current, Vector3Int end)
    {
        GridManager gridManager = References.Instance.GridDataManager;

        return Vector3Int.Distance(current, end) + (gridManager.tempTilesSeenByEnemy.Contains(current)? 1 : 0) + (current.y != end.y? 1 : 0);
    }
}
