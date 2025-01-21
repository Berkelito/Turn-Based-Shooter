using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Grid Data", fileName = "New Grid Data")]
public class GridData : ScriptableObject
{
    [HideInInspector] public SerializableDictionary<Vector3Int, GridTileData> tiles = new ();

    public Vector3Int Bounds;

    [SerializeField] private LayerMask obstacleMask;

    private int TileSize;

    public void ClearSeenTiles()
    {
        foreach (var tile in tiles.Keys)
        {
            tiles[tile].WasSeen = false;
        }
    }

    public void MakeGrid(Vector3Int halfSize, int tileSize)
    {
        tiles.Clear();
        TileSize = tileSize;
        for (int x = -halfSize.x; x < halfSize.x + 1; x++)
        {
            for (int z = -halfSize.z; z < halfSize.z + 1; z++)
            {
                Vector3 checkPosition = new Vector3Int(x, 0, z) * tileSize + new Vector3Int(1, 0, 1) * tileSize / 2;
                checkPosition.y = halfSize.y;

                RaycastHit[] cols = Physics.RaycastAll(checkPosition, Vector3.down, halfSize.y * 5, obstacleMask);

                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i].transform.tag == "Wall")
                    {
                        int yPos = cols[i].transform.GetComponent<FloorRenderBox>().YPos;
                        Vector3Int tilePos = new Vector3Int(x + halfSize.x, yPos, z + halfSize.z);
                        if (!tiles.TryAdd(tilePos, new GridTileData(false)))
                        {
                            tiles[tilePos].IsPassable = false;
                        }
                    }
                    else if (cols[i].transform.tag == "Floor")
                    {
                        int yPos = cols[i].transform.GetComponent<FloorRenderBox>().YPos;
                        tiles.TryAdd(new Vector3Int(x + halfSize.x, yPos, z + halfSize.z), new GridTileData(true));
                    }
                }

            }
        }
        this.Bounds = halfSize;
        this.Bounds.y = 0;

        foreach (var tile in tiles.Keys)
        {
            SetSeenTiles(tile, 13);
        }

        Debug.Log("Grid Created");
    }

    public void SetSeenTiles(Vector3Int tile, float maxDistance)
    {
        for (int x = tile.x - (int)maxDistance; x < tile.x + (int)maxDistance + 1; x++)
        {
            for (int y = tile.y - (int)maxDistance; y < tile.y + (int)maxDistance + 1; y++)
            {
                for (int z = tile.z - (int)maxDistance; z < tile.z + (int)maxDistance + 1; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if (!tiles.ContainsKey(new Vector3Int(x, y, z))) 
                        continue;
                    if (Vector3Int.Distance(pos, tile) <= maxDistance && (CheckIfObstructed(pos, tile + Vector3Int.up, !tiles[pos].IsPassable) 
                        || CheckIfObstructed(pos + Vector3Int.up, tile + Vector3Int.up, false))) 
                    {
                        tiles[tile].SeenTiles.Add(pos, new GridTileData());
                    }
                }
            }
        }
    }


    private bool CheckIfObstructed(Vector3Int start, Vector3Int end, bool ignoreSelf = true) //true if not obstructed
    {
        Vector3 realStart = GridToReal(start);
        Vector3 realEnd = GridToReal(end);

        Vector3 direction = (realEnd - realStart).normalized;
        float distance = Vector3.Distance(realStart, realEnd);


        RaycastHit[] cols = Physics.SphereCastAll(realStart + (ignoreSelf? direction * 16 : Vector3.zero), 0.1f, direction, distance, obstacleMask);  

        if (distance < (direction * 64).magnitude && ignoreSelf) //this is a solution to the problem with sight detection on walls
            return true;                                         //its not very good, but it works and i cant come up with anything else

        foreach (var hit in cols)
        {
            if (ignoreSelf && (Vector3.Distance(hit.point, realStart) <= (TileSize / 2)))
            {
                continue;
            }
            return false;
        }

        return true;
    }

    public Vector3 GridToReal(Vector3Int gridPos)
    {
        Vector3 realPos = (gridPos - Bounds) * TileSize + new Vector3(1, 0, 1) * TileSize / 2;
        return realPos;
    }
}

[System.Serializable]
public class GridTileData
{

    [SerializeField]
    public bool IsPassable = true;

    [SerializeField]
    public bool WasSeen = false;

    [SerializeField]
    public GridEntity OccupyingEntity = null;

    [SerializeField] 
    public SerializableDictionary<Vector3Int, GridTileData> SeenTiles = new ();

    public GridTileData() { }

    public GridTileData(bool isPassable)
    {
        IsPassable = isPassable;

        WasSeen = false;
    }
}
