using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class Enemy : GridEntity
{
    public enum Awareness
    { 
        UnAware,
        Aware,
        Alarmed
    }

    public Awareness awareness = Awareness.UnAware;

    public EnemySystem enemySystem;

    public List<Vector3Int> PatrolRoute = new List<Vector3Int>(); 

    [SerializeField] protected Animator animator;
    
    [SerializeField] protected float spotRange = 10;
    [SerializeField] protected float seeAngle = 0.5f;

    [SerializeField] protected Transform awarenessVisual;

    [SerializeField] private Transform normalVisual;
    [SerializeField] private Transform fowVisual;
    
    protected int patrolRouteIndex = 0;

    protected float awarenessMeter = 0;

    
    protected Vector3Int destination;

    protected Vector3 lookDirection;

    public void SetFogOfWarVisual()
    {
        GridManager gridManager = References.Instance.GridDataManager;
        if (Vector3Int.Distance(GridPosition, References.Instance.Player.GridPosition) > References.Instance.Player.SightRange)
        {
            normalVisual.gameObject.SetActive(false);
            fowVisual.gameObject.SetActive(false);
            awarenessVisual.gameObject.SetActive(false);
        }
        else if (gridManager.TileData.tiles[References.Instance.Player.GridPosition].SeenTiles.ContainsKey(GridPosition))
        {
            normalVisual.gameObject.SetActive(true);
            fowVisual.gameObject.SetActive(false);
            awarenessVisual.gameObject.SetActive(true);
        }
        else
        {
            fowVisual.gameObject.SetActive(true);
            normalVisual.gameObject.SetActive(false);
            awarenessVisual.gameObject.SetActive(false);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (Health <= 0)
        {
            enemySystem.DeleteFromList(this);
            StopAllCoroutines();
            References.Instance.GridDataManager.TileData.tiles[GridPosition].OccupyingEntity = null;
            Destroy(gameObject);
            return;
        }
        animator.SetTrigger("Hit");
    }

    public bool CheckIfSeesTile(Vector3Int pos)
    {
        Vector3 dirToTile = pos - GridPosition;
        if (pos == GridPosition)
            return true;

        GridManager gridManager = References.Instance.GridDataManager;

        float distaceToPos = Vector3Int.Distance(pos, GridPosition);
        float lookDot = Vector3.Dot(lookDirection, dirToTile.normalized);

        return gridManager.TileData.tiles[pos].SeenTiles.ContainsKey(GridPosition)
            && (lookDot >= seeAngle || (distaceToPos < 2 && lookDot >= -0.75f))
            && distaceToPos <= spotRange;
    }

    public virtual void PerformTurnUnAware()
    { 
    
    }
    public virtual void PerformTurnAware()
    {

    }
    public virtual void PerformTurnAlarmed()
    {

    }

    public virtual void SetPerceivedPlayerPos(Vector3Int pos)
    {
        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Enemy), true)]
public class EnemyCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Add This Position To Patrol"))
        {
            Enemy enemy = (target as Enemy);
            enemy.GridPosition = References.Instance.GridDataManager.RealToGrid(enemy.transform.position);
            enemy.PatrolRoute.Add(enemy.GridPosition);
        }
        if (GUILayout.Button("Add Path From Last Patrol Position"))
        {
            Enemy enemy = (target as Enemy);
            enemy.GridPosition = References.Instance.GridDataManager.RealToGrid(enemy.transform.position);
            enemy.PatrolRoute.AddRange(References.Instance.Pathfinding.FindPath(enemy.PatrolRoute[^1], enemy.GridPosition));
        }
        if (GUILayout.Button("Set Grid Position"))
        {
            Enemy enemy = (target as Enemy);
            enemy.GridPosition = References.Instance.GridDataManager.RealToGrid(enemy.transform.position);
        }
    }
}
#endif