using UnityEngine;

public class GridEntity : MonoBehaviour
{
    public Vector3Int GridPosition;

    public bool InAnim = false;

    [SerializeField] protected float Health = 3;

    public virtual void TakeDamage(float damage)
    {
        Health -= damage;
    }

    protected bool TryTakeTile(Vector3Int position)
    {
        GridManager gridManager = References.Instance.GridDataManager;

        if (gridManager.TileData.tiles.TryGetValue(position, out GridTileData tile))
        {
            if (tile.OccupyingEntity != null && tile.OccupyingEntity.TryProtectCurrentTile(this))
            { 
                return false;
            }
            return true;
        }
        return false;
    }

    public virtual bool TryProtectCurrentTile(GridEntity attacker) //true if managed to defend
    {
        return false;
    }
}
