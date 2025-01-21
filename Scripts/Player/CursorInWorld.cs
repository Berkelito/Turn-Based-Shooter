using UnityEngine;

public class CursorInWorld : MonoBehaviour
{
    [SerializeField] private LayerMask layers;

    void Update()
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);

        RaycastHit rayInfo;
        Vector3 dir = -Camera.main.transform.position + Camera.main.ScreenToWorldPoint(mousePosition);
        Physics.Raycast(Camera.main.transform.position, dir, out rayInfo, 500, layers);

        Vector3 newPosition = new Vector3(rayInfo.point.x, 0, rayInfo.point.z);

        transform.position = newPosition;
        
        if (rayInfo.transform != null && rayInfo.transform.TryGetComponent<FloorRenderBox>(out FloorRenderBox box)) //i dont want to do raycastall for better optimization, so this is my workaround
        {
            newPosition.y = box.YPos * References.Instance.GridDataManager.TileSize;
            Vector3Int gridPos = References.Instance.GridDataManager.RealToGrid(newPosition);
            if (References.Instance.GridDataManager.TileData.tiles.ContainsKey(gridPos) && !References.Instance.GridDataManager.TileData.tiles[gridPos].WasSeen)
            {
                Physics.Raycast(newPosition + dir * 4, dir, out rayInfo, 500, layers);
                newPosition = new Vector3(rayInfo.point.x, 0, rayInfo.point.z);
            }
            if (newPosition != Vector3.zero)
                transform.position = newPosition;
        }
    }
}
