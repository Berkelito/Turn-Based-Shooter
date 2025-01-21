using UnityEngine;
using System.Collections.Generic;

public class EnemySystem : MonoBehaviour, ITurnBased
{
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();

    void Start()
    { 
        References.Instance.GameManager.AddToList(this);
        foreach (var enemy in enemies)
        {
            enemy.enemySystem = this;
        }
    }

    public void PerformTurn()
    {
        foreach (var enemy in enemies)
        {
            enemy.SetFogOfWarVisual();

            if (enemy.awareness == Enemy.Awareness.UnAware)
            {
                enemy.PerformTurnUnAware();
            }
            else if (enemy.awareness == Enemy.Awareness.Aware)
            {
                enemy.PerformTurnAware();
            }
            else if (enemy.awareness == Enemy.Awareness.Alarmed)
            { 
                enemy.PerformTurnAlarmed();
            }
        }

        if (enemies.Count == 0) //delete later, only for the prototype
        {
            References.Instance.PlayerUIManager.SetVictoryText(References.Instance.GameManager.TurnCount);
        }

    }

    public void AlarmEveryone(Vector3Int playerPos)
    {
        List<Vector3Int> checkPoses = new List<Vector3Int>();
        checkPoses.Add(playerPos);

        List<Enemy> alarmedEnemies = new List<Enemy>();
        alarmedEnemies.AddRange(enemies);


        List<Vector3Int> futureCheckPoses = new();

        List<Vector3Int> oldPoses = new();

        int maxPushers = Mathf.CeilToInt(enemies.Count / 2);
        int maxIterations = 7;
        while (maxIterations > 0)
        {
            maxIterations--;
            for (int i = 0; i < checkPoses.Count; i++)
            {
                Enemy enemy = CheckIfCanSeenByEnemies(checkPoses[i], alarmedEnemies);
                if (enemy != null)
                {
                    enemy.SetPerceivedPlayerPos(checkPoses[i]);
                    alarmedEnemies.Remove(enemy);
                }

                oldPoses.Add(checkPoses[i]);

                bool addedAnythingFlag = false;
                foreach (var neighbour in References.Instance.GridDataManager.GetNeighbours(checkPoses[i]))
                {
                    if (References.Instance.GridDataManager.TileData.tiles.ContainsKey(neighbour) && 
                        !oldPoses.Contains(neighbour) && !futureCheckPoses.Contains(neighbour)
                        && References.Instance.GridDataManager.TileData.tiles[neighbour].IsPassable)
                    {
                        futureCheckPoses.Add(neighbour);
                        addedAnythingFlag = true;
                    }
                }

                if (!addedAnythingFlag && maxIterations < 4 && alarmedEnemies.Count > maxPushers)
                {
                    Enemy chosenEnemy = FindClosestEnemy(checkPoses[i], alarmedEnemies);
                    chosenEnemy.SetPerceivedPlayerPos(checkPoses[i]);
                    alarmedEnemies.Remove(chosenEnemy);
                }
            }
            checkPoses.Clear();
            checkPoses.AddRange(futureCheckPoses);

            futureCheckPoses.Clear();

        }
        foreach (var enemy in alarmedEnemies)
        {
            enemy.SetPerceivedPlayerPos(playerPos);
        }


    }

    private Enemy FindClosestEnemy(Vector3Int pos, List<Enemy> enemiesToCheck)
    {
        Enemy closestEnemy = null;
        float minDistance = 10000;
        foreach (var enemy in enemiesToCheck)
        {
            float distance = Vector3Int.Distance(pos, enemy.GridPosition);
            if (distance < minDistance)
            {
                closestEnemy = enemy;
                minDistance = distance;
            }
        }
        return closestEnemy;
    }

    private Enemy CheckIfCanSeenByEnemies(Vector3Int pos, List<Enemy> enemiesToCheck)
    {
        GridManager gridManager = References.Instance.GridDataManager;
        foreach (var enemy in enemiesToCheck)
        { 
            if (gridManager.TileData.tiles[pos].SeenTiles.ContainsKey(enemy.GridPosition))
                return enemy;
        }
        return null;
    }

    public bool CheckIfSeenByEnemies(Vector3Int pos)
    {
        foreach (var enemy in enemies)
        {
            if (enemy.CheckIfSeesTile(pos))
                return true;
        }
        return false;
    }


    public void DeleteFromList(Enemy enemy)
    { 
        enemies.Remove(enemy);
    }
}
