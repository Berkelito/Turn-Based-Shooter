using UnityEngine;
using UnityEditor;

public class References : MonoBehaviour
{
    public PlayerActionController Player;
    public PlayerUI PlayerUIManager;
    public FloorRender FloorRenderer;
    public CursorInWorld CursorInWorld;
    public CinemaCameraController CinemaCamera;
    public GridManager GridDataManager;
    public Astar Pathfinding;
    public TurnManager GameManager;
    public SceneGameManager SceneManager;

    public static References Instance;

    public void Awake()
    {
        Instance = this; 
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(References))]
public class ReferencesCustomEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Initiate"))//it makes it so i can use the references in other custom inspectors
        {
            (target as References).Awake();
        }
    }
}
#endif