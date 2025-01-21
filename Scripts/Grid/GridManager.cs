using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GridData TileData;

    public int TileSize = 15;

    public List<Vector3Int> tempTilesSeenByEnemy = new List<Vector3Int>();

    private void Start()
    {
        TileData.ClearSeenTiles();
    }

    public Vector3Int RealToGrid(Vector3 realPos)
    {
        Vector3Int gridPos = Vector3Int.FloorToInt(realPos / TileSize) + TileData.Bounds;
        return gridPos;
    }
    public Vector3 GridToReal(Vector3Int gridPos)
    { 
        Vector3 realPos = (gridPos - TileData.Bounds) * TileSize + new Vector3(1, 0, 1) * TileSize / 2;
        return realPos;
    }

    public Vector3Int[] GetNeighbours(Vector3Int pos)
    {
        List<Vector3Int> neighbours = new ();
        for (int i = 0; i < 4; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (TileData.tiles.ContainsKey(pos - Vector3Int.right + new Vector3Int(0, j, 0)))
                    neighbours.Add(pos - Vector3Int.right + new Vector3Int(0,j,0));

                if (TileData.tiles.ContainsKey(pos + Vector3Int.right + new Vector3Int(0, j, 0)))
                    neighbours.Add(pos + Vector3Int.right + new Vector3Int(0, j, 0));

                if (TileData.tiles.ContainsKey(pos - Vector3Int.forward + new Vector3Int(0, j, 0)))
                    neighbours.Add(pos - Vector3Int.forward + new Vector3Int(0, j, 0));

                if (TileData.tiles.ContainsKey(pos + Vector3Int.forward + new Vector3Int(0, j, 0)))
                    neighbours.Add(pos + Vector3Int.forward + new Vector3Int(0, j, 0));
            }
        }

        return neighbours.ToArray();
    }

    public Tile[] GetSeenTiles(Vector3Int tile, float maxDistance, List<ITurnBased> turnableEntities)
    {
        List<Tile> tiles = new();
        tempTilesSeenByEnemy.Clear();
        for (int x = tile.x - (int)maxDistance; x < tile.x + (int)maxDistance + 1; x++)
        {
            for (int y = tile.y - (int)maxDistance; y < tile.y + (int)maxDistance + 1; y++)
            {
                for (int z = tile.z - (int)maxDistance; z < tile.z + (int)maxDistance + 1; z++)
                {
                    if (!TileData.tiles.ContainsKey(new Vector3Int(x, y, z)))
                        continue;

                    Tile newTile = new Tile();


                    newTile.Position = new Vector3Int(x, y, z);

                    if (!TileData.tiles[tile].SeenTiles.ContainsKey(new Vector3Int(x, y, z)))
                    {
                        newTile.State = 0;
                    }
                    else
                    {
                        newTile.State = 1;


                        TileData.tiles[newTile.Position].WasSeen = true;
                       

                        foreach (var entity in turnableEntities)
                        {
                            if ((entity is EnemySystem) && (entity as EnemySystem).CheckIfSeenByEnemies(newTile.Position))
                            {
                                newTile.State = 0.2f;
                                tempTilesSeenByEnemy.Add(newTile.Position);
                                break;
                            }
                        }
                    }
                    tiles.Add(newTile);

                }
            }
        }
        return tiles.ToArray();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private Vector3Int searchSize;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Search size: ");
            searchSize.x = EditorGUILayout.IntField(searchSize.x);
            searchSize.y = EditorGUILayout.IntField(searchSize.y);
            searchSize.z = EditorGUILayout.IntField(searchSize.z);
        EditorGUILayout.EndHorizontal();


        if (GUILayout.Button("Search"))
        {
            GridManager gridManager = (GridManager)target;

            EditorUtility.SetDirty(gridManager.TileData);

            gridManager.TileData.MakeGrid(searchSize / 2, gridManager.TileSize);
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
        if (GUILayout.Button("ReSearch"))
        {
            GridManager gridManager = (GridManager)target;

            EditorUtility.SetDirty(gridManager.TileData);

            gridManager.TileData.MakeGrid(gridManager.TileData.Bounds + Vector3Int.up * 512, gridManager.TileSize);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("Clear seen tiles"))
        {
            GridManager gridManager = (GridManager)target;

            EditorUtility.SetDirty(gridManager.TileData);

            gridManager.TileData.ClearSeenTiles();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif