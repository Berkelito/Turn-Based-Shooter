using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BasicEnemy : Enemy
{
    [SerializeField] private float stepTime = 0.5f;

    [SerializeField] private Transform laser;

    private List<Vector3Int> walkPath = new List<Vector3Int>();
    private int walkIndex = 0;

    [SerializeField] private Vector3Int perceivedPlayerPos = Vector3Int.zero;
    [SerializeField] private LayerMask playerMask;

    private Vector3 originalLookDirection = Vector3.zero;

    private bool isShooting = false;

    private void Start()
    {
        lookDirection = transform.forward;
        originalLookDirection = transform.forward;
        transform.position = References.Instance.GridDataManager.GridToReal(GridPosition);
        References.Instance.GridDataManager.TileData.tiles[GridPosition].OccupyingEntity = this;
    }

    public override void PerformTurnUnAware()
    {
        destination = PatrolRoute[patrolRouteIndex];
        patrolRouteIndex++;
        if (patrolRouteIndex >= PatrolRoute.Count)
            patrolRouteIndex = 0;

        if (PatrolRoute.Count == 0 || PatrolRoute.Count == 1)
        {
            lookDirection = originalLookDirection;
            transform.forward = lookDirection;
        }

        StartCoroutine(Move());

        if (CheckIfSeesTile(References.Instance.Player.GridPosition))
        {
            awarenessMeter += 100;
            if (awarenessMeter >= 100)
            {
                awareness = Awareness.Aware;
                walkPath = References.Instance.Pathfinding.FindPath(GridPosition, References.Instance.Player.GridPosition);
                walkIndex = 0;

                awarenessVisual.gameObject.SetActive(true);
                awarenessVisual.GetComponent<TMP_Text>().color = Color.yellow;
            }
        }
    }

    public override void PerformTurnAware()
    {
        if (walkIndex < walkPath.Count)
        {
            destination = walkPath[walkIndex];
            walkIndex++;
        }
        StartCoroutine(Move());

        if (CheckIfSeesTile(References.Instance.Player.GridPosition))
        {
            awarenessMeter += 100;
            if (awarenessMeter >= 200)
            {
                enemySystem.AlarmEveryone(References.Instance.Player.GridPosition);
                awareness = Awareness.Alarmed;
                walkPath.Clear();
                walkIndex = 0;

                awarenessVisual.GetComponent<TMP_Text>().color = Color.red;
            }
        }
        else
        {
            awarenessMeter -= 25;

            if (GridPosition == walkPath[^1])
            {
                awarenessMeter -= 25;
            }

            if (awarenessMeter <= 0)
            {
                awarenessMeter = 0;
                if (walkPath[^1] != PatrolRoute[patrolRouteIndex])
                {
                    walkPath = References.Instance.Pathfinding.FindPath(GridPosition, PatrolRoute[patrolRouteIndex]);
                    walkIndex = 0;
                }
                if (GridPosition == PatrolRoute[patrolRouteIndex])
                {
                    awareness = Awareness.UnAware;

                    awarenessVisual.GetComponent<TMP_Text>().color = new Color(0,0,0,0);
                }
            }
        }
    }

    public override void PerformTurnAlarmed()
    {
        GridManager gridManager = References.Instance.GridDataManager;

        if (isShooting)
        {
            Vector3 perceivedRealPos = gridManager.GridToReal(perceivedPlayerPos) + Vector3.up * 3;
            Vector3 realPos = transform.position + Vector3.up * 3;

            if (Physics.SphereCast(realPos, 5, perceivedRealPos - realPos, out RaycastHit hitInfo, Vector3.Distance(realPos, perceivedRealPos) * 2, playerMask))
            {
                gridManager.TileData.tiles[References.Instance.Player.GridPosition].OccupyingEntity?.TakeDamage(1);
            }
            isShooting = false;
            laser.gameObject.SetActive(false);
        }
        if (!isShooting && CheckIfSeesTile(References.Instance.Player.GridPosition))
        {
            isShooting = true;
            perceivedPlayerPos = References.Instance.Player.GridPosition;
            enemySystem.AlarmEveryone(perceivedPlayerPos);


            laser.gameObject.SetActive(true);
            laser.GetComponent<LineController>().targetPosition = gridManager.GridToReal(perceivedPlayerPos);

            lookDirection = Vector3.Normalize(perceivedPlayerPos - GridPosition);
            transform.forward = lookDirection;
            return;
        }
        

        if (walkIndex < walkPath.Count)
        {
            destination = walkPath[walkIndex];
            walkIndex++;
            StartCoroutine(Move());
        }
        else 
        {
            lookDirection = Vector3.Normalize(perceivedPlayerPos - GridPosition);
            transform.forward = lookDirection;
        }


    }

    public override bool TryProtectCurrentTile(GridEntity attacker)
    {
        if (attacker is Enemy)
            return true;
        References.Instance.GameManager.MakeNoise(GridPosition, 5);
        TakeDamage(Health);
        return false;
    }

    public override void SetPerceivedPlayerPos(Vector3Int pos)
    {
        awareness = Awareness.Alarmed;
        awarenessMeter = Mathf.Clamp(awarenessMeter, 200, 300);
        perceivedPlayerPos = pos;

        awarenessVisual.gameObject.SetActive(true);
        awarenessVisual.GetComponent<TMP_Text>().color = Color.red;

        walkPath = References.Instance.Pathfinding.FindPath(GridPosition, FindBestPosition(perceivedPlayerPos));
        walkIndex = 0;
    }

    private Vector3Int FindBestPosition(Vector3Int playerPos)
    {
        List<Vector3Int> checkPoses = new List<Vector3Int>();
        checkPoses.Add(GridPosition);

        List<Vector3Int> futureCheckPoses = new();

        List<Vector3Int> oldPoses = new();

        Vector3Int reservedPos = playerPos; //in case nothing is found

        int maxIterations = 10;
        while (maxIterations > 0)
        {
            maxIterations--;
            for (int i = 0; i < checkPoses.Count; i++)
            {
                if (CheckCover(checkPoses[i], playerPos))
                {
                    return checkPoses[i];
                }

                if (References.Instance.GridDataManager.TileData.tiles[playerPos].SeenTiles.ContainsKey(checkPoses[i]))
                    reservedPos = checkPoses[i];

                oldPoses.Add(checkPoses[i]);
                
                foreach (var neighbour in References.Instance.GridDataManager.GetNeighbours(checkPoses[i]))
                {
                    if (!oldPoses.Contains(neighbour) && !futureCheckPoses.Contains(neighbour) 
                        && References.Instance.GridDataManager.TileData.tiles[neighbour].IsPassable)
                    {
                        futureCheckPoses.Add(neighbour);
                    }
                }
            }
            checkPoses.Clear();
            checkPoses.AddRange(futureCheckPoses);

            futureCheckPoses.Clear();

        }
        return reservedPos;
    }

    private bool CheckCover(Vector3Int tilePos, Vector3Int playerPos)
    {
        GridData gridData = References.Instance.GridDataManager.TileData;

        if (!gridData.tiles[playerPos].SeenTiles.ContainsKey(tilePos) || Vector3Int.Distance(tilePos, playerPos) > spotRange)//initial tile must be seen by player
        {
            return false;
        }

        foreach (var tile in References.Instance.GridDataManager.GetNeighbours(tilePos))
        {
            if (!gridData.tiles[playerPos].SeenTiles.ContainsKey(tile) && gridData.tiles[tile].IsPassable) //is not seen by player
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator Move()
    {
        GridManager gridManager = References.Instance.GridDataManager;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = References.Instance.GridDataManager.GridToReal(destination);

        if (gridManager.RealToGrid(startPosition) == destination)
        {
            yield return new WaitForSeconds(stepTime);
            yield break;
        }
        
        if (TryTakeTile(destination))
        {
            gridManager.TileData.tiles[gridManager.RealToGrid(startPosition)].OccupyingEntity = null;
            gridManager.TileData.tiles[destination].OccupyingEntity = this;
            GridPosition = destination;
        }
        else
        {
            GridPosition = gridManager.RealToGrid(startPosition);
            yield break;
        }


        if (awareness == Awareness.Alarmed)
        {
            lookDirection = Vector3.Normalize(perceivedPlayerPos + Vector3Int.up - GridPosition);

            if (laser.gameObject.activeInHierarchy)
                laser.GetComponent<LineController>().targetPosition = gridManager.GridToReal(perceivedPlayerPos);
        }
        else
            lookDirection = (targetPosition - startPosition).normalized;

        transform.forward = lookDirection;

        for (float i = 0; i <= 1; i += Time.deltaTime / stepTime)
        {
            transform.position = startPosition + ((targetPosition - startPosition) * Mathf.Sin(i / 2 * Mathf.PI));
            yield return null;
        }
        transform.position = targetPosition;
       
    }
}