using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

public class PlayerActionController : GridEntity
{
    public float SightRange = 13;

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Animator animator;

    [SerializeField] private float stepTime = 0.5f;
    [SerializeField] private int maxStepsPerTurn = 2;

    [SerializeField] private GameObject pathVisual;

    [SerializeField] private Transform collisionBox; // collision and visuals are separated, so the collision wouldnt move with the visuals

    private List<Vector3Int> currentPath = new();
    private List<GameObject> spawnedPathVisuals = new();

    private Vector3Int lookedAtPosition = Vector3Int.zero;

    private void Start()
    {
        GridPosition = References.Instance.GridDataManager.RealToGrid(transform.position);

        transform.position = References.Instance.GridDataManager.GridToReal(GridPosition);

        playerInput.actions.FindAction("ClickMove").performed += Click;

        playerInput.actions.FindAction("Interact").performed += Interact;

        playerInput.actions.FindAction("Menu").performed += ToggleMenu;

        StartCoroutine(LateTurn());
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed || Time.timeScale == 0 || InAnim 
            || lookedAtPosition == Vector3Int.zero
            || !References.Instance.GridDataManager.TileData.tiles[GridPosition].SeenTiles.ContainsKey(lookedAtPosition))
            return;

        References.Instance.GridDataManager.TileData.tiles[lookedAtPosition].OccupyingEntity?.TakeDamage(1);

        References.Instance.GameManager.MakeNoise(GridPosition, 20);

        References.Instance.GameManager.NextTurn();
    }

    private void Click(InputAction.CallbackContext context)
    {
        if (!context.performed || Time.timeScale == 0 || InAnim || lookedAtPosition == Vector3Int.zero)
            return;

        StartCoroutine(MoveAlongPath(currentPath));
    }
    private void ToggleMenu(InputAction.CallbackContext context)
    {
        if (!context.performed || References.Instance.PlayerUIManager.deadMenu.gameObject.activeInHierarchy)
            return;

        References.Instance.PlayerUIManager.quickMenu.ToggleObject(References.Instance.PlayerUIManager.quickMenu.gameObject);
    }

    IEnumerator LateTurn()
    {
        yield return null;
        References.Instance.GameManager.NextTurn();
    }

    public override void TakeDamage(float damage)
    {
        PlayerUI uiManager = References.Instance.PlayerUIManager;

        Health -= damage;
        uiManager.SetHealthBar(Health);
        if (Health <= 0)
        {
            uiManager.deadMenu.ToggleObject(uiManager.deadMenu.gameObject);
            return;
        }
        animator.SetTrigger("Hit");
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;
        ShowPath();
    }

    void ShowPath()
    {
        GridManager gridManager = References.Instance.GridDataManager;
        Vector3Int goPosition = gridManager.RealToGrid(References.Instance.CursorInWorld.transform.position);

        if (!InAnim && goPosition != lookedAtPosition && gridManager.TileData.tiles.ContainsKey(goPosition) 
            && gridManager.TileData.tiles[goPosition].IsPassable && gridManager.TileData.tiles[goPosition].WasSeen)
        {
            lookedAtPosition = goPosition;
            currentPath = References.Instance.Pathfinding.FindPath(gridManager.RealToGrid(transform.position), goPosition, false);
            
            for (int i = spawnedPathVisuals.Count - 1; i >= 0; i--) //constant destroying and instantiating new object is not performent, this is a good place to do some optimization
            {
                Destroy(spawnedPathVisuals[i], 0.01f);
                spawnedPathVisuals.RemoveAt(i);
            }

            int cost = 1;
            for (int i = 0; i < currentPath.Count; i++)
            {
                GameObject node = Instantiate(pathVisual, gridManager.GridToReal(currentPath[i]), Quaternion.identity);
                node.GetComponent<WalkNode>().Cost = cost;
                spawnedPathVisuals.Add(node);

                if ((i - 1) % maxStepsPerTurn == 0)
                    cost++;
            }
        }
    }

    private IEnumerator MoveAlongPath(List<Vector3Int> path)
    {
        GridManager gridManager = References.Instance.GridDataManager;
        int stepCounter = 0;
        InAnim = true;
        foreach (var tile in path)
        {
            if (!gridManager.TileData.tiles[tile].IsPassable)
            {
                References.Instance.GameManager.NextTurn();
                yield return null;

                currentPath = References.Instance.Pathfinding.FindPath(gridManager.RealToGrid(transform.position), path[^1]);
                StartCoroutine(MoveAlongPath(currentPath));
                yield break;
            }

            Vector3 targetPosition = gridManager.GridToReal(tile);
            Vector3 startPosition = transform.position;

            if (TryTakeTile(tile))
            {
                gridManager.TileData.tiles[gridManager.RealToGrid(startPosition)].OccupyingEntity = null;
                gridManager.TileData.tiles[tile].OccupyingEntity = this;
            }
            else
            {
                GridPosition = gridManager.RealToGrid(startPosition);
                InAnim = false;
                References.Instance.GameManager.NextTurn();
                yield break;
            }

            collisionBox.transform.position = targetPosition;

            stepCounter++;
            if (stepCounter >= maxStepsPerTurn)
            {
                stepCounter = 0;
                GridPosition = tile;

                yield return new WaitForFixedUpdate();

                References.Instance.GameManager.NextTurn();
            }


            for (float i = 0; i <= 1; i += Time.deltaTime / stepTime)
            {
                collisionBox.transform.position = targetPosition;

                transform.position = startPosition + ((targetPosition - startPosition) * Mathf.Sin(i / 2 * Mathf.PI));
                yield return null;
            }
            transform.position = targetPosition;
            collisionBox.transform.position = targetPosition;
        }
        GridPosition = gridManager.RealToGrid(transform.position);
        if (stepCounter > 0 && stepCounter < maxStepsPerTurn)
        {
            References.Instance.GameManager.NextTurn();
        }
        currentPath.Clear();
        InAnim = false;
    }

    public override bool TryProtectCurrentTile(GridEntity attacker)
    {
        return true;
    }
    private void OnDestroy()
    {
        playerInput.actions.FindAction("ClickMove").performed -= Click;

        playerInput.actions.FindAction("Interact").performed -= Interact;

        playerInput.actions.FindAction("Menu").performed -= ToggleMenu;
    }
}
