using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public int TurnCount = 0;

    private List<ITurnBased> entities = new List<ITurnBased>();

    public void AddToList(ITurnBased entity)
    { 
        entities.Add(entity);
    }
    public void RemoveFromList(ITurnBased entity)
    { 
        entities.Remove(entity);
    }

    public void NextTurn()
    {
        foreach (var entity in entities)
        { 
            entity.PerformTurn();
        }
        TurnCount++;

        RenderTiles();
    }

    public void MakeNoise(Vector3Int pos, float range)
    {
        foreach (var entity in entities)
        {
            if (entity is EnemySystem 
                && Vector3.Distance(References.Instance.GridDataManager.RealToGrid((entity as EnemySystem).transform.position), pos) <= range)
            {
                (entity as EnemySystem).AlarmEveryone(pos);
            }
        }
    }

    private void RenderTiles()
    {
        Tile[] tiles = References.Instance.GridDataManager.GetSeenTiles(References.Instance.Player.GridPosition, References.Instance.Player.SightRange + 4, entities);

        References.Instance.FloorRenderer.RenderSeenTiles(tiles);
    }
}

public interface ITurnBased
{ 
    public void PerformTurn();

}