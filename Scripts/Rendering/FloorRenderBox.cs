using UnityEngine;
using UnityEditor;

public class FloorRenderBox : MonoBehaviour
{
    public Vector3 Size;

    public Color LowColor;
    public Color HighColor;

    public int YPos = 0;

    
    [SerializeField] private FloorRender floorRender;
    [SerializeField] private bool invisible = false;

    private void Awake()
    {
        if (invisible)
            return;
        floorRender.Count += Mathf.RoundToInt(Size.x * Size.y * Size.z);
        floorRender.floors.Add(this);

        GetComponent<BoxCollider>().size = Size * floorRender.padding;
        GetComponent<BoxCollider>().center = Vector3.one - Vector3.up * Size.y;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.one - Vector3.up * Size.y, Size * floorRender.padding);
        GetComponent<BoxCollider>().size = Size * floorRender.padding;

        GetComponent<BoxCollider>().center = Vector3.one - Vector3.up * Size.y;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FloorRenderBox)), CanEditMultipleObjects]
public class FloorRenderBoxEditor : Editor
{
    private Vector3Int newSize;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Size: ");
            newSize.x = EditorGUILayout.IntField(newSize.x);
            newSize.y = EditorGUILayout.IntField(newSize.y);
            newSize.z = EditorGUILayout.IntField(newSize.z);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Correct Position"))
        {
            FloorRenderBox renderBox = (target as FloorRenderBox);
            int TileSize = References.Instance.GridDataManager.TileSize;

            Vector3 boxPos = renderBox.transform.position;
            Vector3Int gridPos = Vector3Int.FloorToInt(boxPos / TileSize);
            boxPos = gridPos * TileSize + Vector3.one * TileSize / 2;
            boxPos -= Vector3.one;
            boxPos.y = renderBox.transform.position.y;

            boxPos += new Vector3(renderBox.Size.x % TileSize == 0? TileSize / 2 : 0, 0, renderBox.Size.z % TileSize == 0 ? TileSize / 2 : 0);

            renderBox.transform.position = boxPos;
        }
        if (GUILayout.Button("Correct Size and Position"))
        {
            FloorRenderBox renderBox = (target as FloorRenderBox);
            int TileSize = References.Instance.GridDataManager.TileSize;

            renderBox.Size = newSize * TileSize / 2;

            Vector3 boxPos = renderBox.transform.position;
            Vector3Int gridPos = Vector3Int.FloorToInt(boxPos / TileSize);
            boxPos = gridPos * TileSize + Vector3.one * TileSize / 2;
            boxPos -= Vector3.one;
            boxPos.y = renderBox.transform.position.y;

            boxPos += new Vector3(newSize.x % 2 == 0 ? TileSize / 2 : 0, 0, newSize.z % 2 == 0 ? TileSize / 2 : 0);
            renderBox.transform.position = boxPos;

            if (newSize.y > 1)
            {
                GameObject objectToSpawn = renderBox.gameObject;
                renderBox.Size.y = TileSize / 2;

                while (renderBox.transform.childCount > 0)
                {
                    DestroyImmediate(renderBox.transform.GetChild(0).gameObject);
                }
                for (int i = 1; i < newSize.y; i++)
                {
                    FloorRenderBox box = Instantiate(objectToSpawn, renderBox.transform).GetComponent<FloorRenderBox>();
                    box.Size = renderBox.Size;
                    box.transform.position = boxPos + Vector3.up * i * TileSize;
                    box.YPos = i + renderBox.YPos;

                    while (box.transform.childCount > 0)
                    {
                        DestroyImmediate(box.transform.GetChild(0).gameObject);
                    }
                }
            }
        }
    }
}
#endif
