using UnityEngine;
using UnityEngine.Rendering;

public class GrassInstancer : MonoBehaviour
{
    public Mesh grassMesh;
    public Material grassMaterial;
    public Terrain terrain;
    public float density = 2f;
    public float gridOffset;
    int gridSize;

    public float heightOffset;

    private ComputeBuffer argsBuffer;
    private ComputeBuffer positionBuffer;

    private void Awake()
    {
        TerrainData terrainData = terrain.terrainData;
        float terrainWidth = terrainData.size.x;
        float terrainHeight = terrainData.size.z;
        gridSize = (int)(terrainWidth + terrainHeight)/2 + (int)gridOffset;
    }

    void Start()
    {
        Vector4[] positions = new Vector4[gridSize * gridSize];

        TerrainData terrainData = terrain.terrainData;
        float terrainWidth = terrainData.size.x;
        float terrainHeight = terrainData.size.z;

        Vector3 terrainPosition = terrain.transform.position; // Get terrain position in world space

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                // Calculate positions relative to terrain
                float posX = (x / (float)gridSize) * terrainWidth + terrainPosition.x;
                float posZ = (z / (float)gridSize) * terrainHeight + terrainPosition.z;

                // ✅ Add random offset for a natural look
                posX += Random.Range(-density, density);
                posZ += Random.Range(-density, density);

                // ✅ Sample terrain height correctly
                float posY = terrain.SampleHeight(new Vector3(posX, 0, posZ)) + terrainPosition.y;

                positions[x * gridSize + z] = new Vector4(posX, posY + heightOffset, posZ, 1);
            }
        }

        positionBuffer = new ComputeBuffer(gridSize * gridSize, sizeof(float) * 4);
        positionBuffer.SetData(positions);

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)(gridSize * gridSize), 0, 0, 0 };
        argsBuffer.SetData(args);

        grassMaterial.SetBuffer("_PositionBuffer", positionBuffer);
    }


    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(terrain.transform.position + terrain.terrainData.size / 2, terrain.terrainData.size), argsBuffer);
    }

    void OnDestroy()
    {
        positionBuffer?.Release();
        argsBuffer?.Release();
    }
}