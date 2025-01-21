using UnityEngine;
using System.Collections.Generic;

public class FloorRender : MonoBehaviour
{
    public List<FloorRenderBox> floors = new();

    public int Count = 0;

    public float padding = 2;

    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    [SerializeField] private ComputeShader compute;

    [SerializeField] private float perlinNoiseScale;
    [SerializeField] private float perlinNoiseStrength;
    

    private ComputeBuffer blockBuffer;
    private ComputeBuffer tileBuffer;

    private RenderTexture renderTexture;

    private GraphicsBuffer argsBuffer;
    private void Start()
    {
        blockBuffer?.Release();
        blockBuffer = new ComputeBuffer(Count, 30 * sizeof(float) + sizeof(int) * 3);


        Block[] blockData = new Block[Count];

        int index = 0;
        foreach (var floor in floors)
        {
            for (int x = 0; x < floor.Size.x; x++)
            {
                for (int y = 0; y < floor.Size.y; y++)
                {
                    for (int z = 0; z < floor.Size.z; z++)
                    {
                        Vector3 startPos = new Vector3(x * padding, y * padding, z * padding) + floor.transform.position - floor.Size * padding / 2 + Vector3.one * padding / 2;

                        startPos.y -= floor.Size.y * padding / 2;

                        Vector3Int gridPos = References.Instance.GridDataManager.RealToGrid(startPos);

                        gridPos.y = floor.YPos;

                        float noise = Mathf.PerlinNoise(startPos.x * perlinNoiseScale * padding, startPos.z * perlinNoiseScale * padding) * perlinNoiseStrength;

                        blockData[index].StartMat = startPos;
                        blockData[index].Mat = Matrix4x4.TRS(startPos, Quaternion.identity, Vector3.one);

                        blockData[index].TimeOffset = noise;

                        blockData[index].LowColor = new Vector3(floor.LowColor.r, floor.LowColor.g, floor.LowColor.b);
                        blockData[index].HighColor = new Vector3(floor.HighColor.r, floor.HighColor.g, floor.HighColor.b);

                        blockData[index].GridPos = gridPos;

                        blockData[index].Color = new Color(0.1f, 0.1f, 0.1f, 0);

                        index++;
                    }
                }
            }
        }

        blockBuffer.SetData(blockData);

        renderTexture = new RenderTexture(512, 512, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = 512;
        renderTexture.Create();

        compute.SetTexture(compute.FindKernel("SetTileData"), "tileData", renderTexture);
        compute.SetTexture(compute.FindKernel("CSMain"), "tileData", renderTexture);
        compute.SetBuffer(compute.FindKernel("CSMain"), "data", blockBuffer);
        material.SetBuffer("data", blockBuffer);

        argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 5, sizeof(uint));

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)Count;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        args[4] = 0;

        argsBuffer.SetData(args);
    }

    public void RenderSeenTiles(Tile[] tiles)
    {
        tileBuffer?.Release();

        int size = Count / 4;

        tileBuffer = new ComputeBuffer(size, sizeof(int) * 3 + sizeof(float));

        tileBuffer.SetData(tiles);

        compute.SetBuffer(compute.FindKernel("SetTileData"), "precomputedData", tileBuffer);
        compute.Dispatch(compute.FindKernel("SetTileData"),Mathf.CeilToInt(size / 8), 1, 1);
    }

    private void Update()
    {
        RenderParams rp = new RenderParams();
        rp.worldBounds = new Bounds(transform.position, Vector3.one * Count * padding);
        rp.material = material;
        Graphics.RenderMeshIndirect(rp, mesh, argsBuffer);

        compute.SetVector("PlayerPosition", References.Instance.Player.transform.position);

        compute.Dispatch(compute.FindKernel("CSMain"), Mathf.CeilToInt(Count / 64f), 1, 1);
    }

    private void OnDestroy()
    {
        blockBuffer?.Release();
        tileBuffer?.Release();
        argsBuffer?.Release();

        renderTexture?.Release();
    }
}

public struct Tile
{
    public Vector3Int Position;
    public float State; //0-not seen 1-seen 0.2-seen by enemy

    public Tile(Vector3Int position, float state)
    {
        Position = position;
        State = state;
    }
}

public struct Block
{
    public Vector3 StartMat;
    public Matrix4x4 Mat;

    public Vector3Int GridPos;

    public Vector3 LowColor;
    public Vector3 HighColor;
    public Color Color;

    public float TimeOffset;
}
