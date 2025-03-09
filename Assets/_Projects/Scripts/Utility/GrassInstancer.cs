using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class GrassInstancer : MonoBehaviour
{
    [Header("Mesh and Material")]
    public Mesh grassMesh;
    public Material grassMaterial;
    public Terrain terrain;

    [Header("Density Settings")]
    public float density = 2f;
    public float heightOffset = 0.1f;

    [Header("LOD Settings")]
    public float maxRenderDistance = 100f;

    [Header("Chunking")]
    public int chunkSize = 32;

    // Private fields
    private TerrainData terrainData;
    private Vector3 terrainPosition;
    private Dictionary<Vector2Int, GrassChunk> chunks = new Dictionary<Vector2Int, GrassChunk>();
    private Camera mainCamera;
    private Vector2Int lastCameraChunk = new Vector2Int(-999, -999);
    private int chunksPerTerrainAxis;
    private MaterialPropertyBlock propertyBlock;

    // Debug options
    public bool debugMode = true;

    private class GrassChunk
    {
        public ComputeBuffer positionBuffer;
        public ComputeBuffer argsBuffer;
        public Vector3 centerPosition;
        public Bounds bounds;
        public int instanceCount;

        public void Release()
        {
            if (positionBuffer != null)
            {
                positionBuffer.Release();
                positionBuffer = null;
            }

            if (argsBuffer != null)
            {
                argsBuffer.Release();
                argsBuffer = null;
            }
        }
    }

    private void Awake()
    {
        terrainData = terrain.terrainData;
        terrainPosition = terrain.transform.position;
        mainCamera = Camera.main;

        // Calculate how many chunks we need
        chunksPerTerrainAxis = Mathf.CeilToInt(terrainData.size.x / chunkSize);

        Debug.Log($"Terrain size: {terrainData.size}, Position: {terrainPosition}");
        Debug.Log($"Total chunks needed: {chunksPerTerrainAxis} x {chunksPerTerrainAxis}");
    }

    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        Vector2Int currentCameraChunk = WorldPosToChunkCoord(cameraPos);

        if (debugMode)
        {
            Debug.Log($"Camera at chunk: {currentCameraChunk}, World pos: {cameraPos}");
        }

        // Only rebuild chunks if the camera moved to a different chunk
        if (currentCameraChunk != lastCameraChunk)
        {
            if (debugMode)
            {
                Debug.Log($"Camera moved from chunk {lastCameraChunk} to {currentCameraChunk}");
            }

            // Release chunks that are too far away
            List<Vector2Int> chunksToRemove = new List<Vector2Int>();
            foreach (var kvp in chunks)
            {
                if (Vector2Int.Distance(kvp.Key, currentCameraChunk) > maxRenderDistance / chunkSize)
                {
                    kvp.Value.Release();
                    chunksToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in chunksToRemove)
            {
                chunks.Remove(key);
                if (debugMode) Debug.Log($"Removed chunk {key}");
            }

            // Create new chunks within render distance
            int chunkViewDistance = Mathf.CeilToInt(maxRenderDistance / chunkSize);
            for (int x = -chunkViewDistance; x <= chunkViewDistance; x++)
            {
                for (int z = -chunkViewDistance; z <= chunkViewDistance; z++)
                {
                    Vector2Int chunkCoord = new Vector2Int(
                        currentCameraChunk.x + x,
                        currentCameraChunk.y + z
                    );

                    // Skip if chunk is outside terrain bounds
                    if (chunkCoord.x < 0 || chunkCoord.y < 0 ||
                        chunkCoord.x >= chunksPerTerrainAxis || chunkCoord.y >= chunksPerTerrainAxis)
                        continue;

                    float distanceFromCameraChunk = Vector2Int.Distance(chunkCoord, currentCameraChunk);

                    // Skip if too far
                    if (distanceFromCameraChunk > chunkViewDistance)
                        continue;

                    // Create chunk if it doesn't exist
                    if (!chunks.ContainsKey(chunkCoord))
                    {
                        CreateChunk(chunkCoord, distanceFromCameraChunk);
                        if (debugMode) Debug.Log($"Created chunk {chunkCoord}");
                    }
                }
            }

            lastCameraChunk = currentCameraChunk;
        }

        // Draw all chunks
        foreach (var chunk in chunks.Values)
        {
            if (chunk.positionBuffer != null && chunk.argsBuffer != null)
            {
                // Set up property block for this specific chunk
                propertyBlock.Clear();
                propertyBlock.SetBuffer("_PositionBuffer", chunk.positionBuffer);

                // Draw this chunk
                Graphics.DrawMeshInstancedIndirect(
                    grassMesh, 0, grassMaterial,
                    chunk.bounds, chunk.argsBuffer, 0, propertyBlock);
            }
        }
    }

    private Vector2Int WorldPosToChunkCoord(Vector3 worldPos)
    {
        // Convert to terrain-local position
        Vector3 localPos = worldPos - terrainPosition;

        int chunkX = Mathf.FloorToInt(localPos.x / chunkSize);
        int chunkZ = Mathf.FloorToInt(localPos.z / chunkSize);

        return new Vector2Int(chunkX, chunkZ);
    }

    private void CreateChunk(Vector2Int chunkCoord, float distanceFromCamera)
    {
        // Calculate world position of chunk corner
        float worldX = chunkCoord.x * chunkSize + terrainPosition.x;
        float worldZ = chunkCoord.y * chunkSize + terrainPosition.z;

        // Sample heights at corners to get accurate height range
        float heightMin = float.MaxValue;
        float heightMax = float.MinValue;

        // Sample several points to get height range
        for (int x = 0; x <= 4; x++)
        {
            for (int z = 0; z <= 4; z++)
            {
                float sampleX = worldX + (x / 4f) * chunkSize;
                float sampleZ = worldZ + (z / 4f) * chunkSize;
                float height = terrain.SampleHeight(new Vector3(sampleX, 0, sampleZ)) + terrainPosition.y;

                heightMin = Mathf.Min(heightMin, height);
                heightMax = Mathf.Max(heightMax, height);
            }
        }

        // Add padding to height range
        heightMin -= 5f;
        heightMax += 10f;

        // Calculate center position for bounds
        Vector3 centerPos = new Vector3(
            worldX + chunkSize / 2,
            (heightMin + heightMax) / 2,
            worldZ + chunkSize / 2
        );

        // Create bounds that accurately encompass the entire chunk
        Vector3 boundsSize = new Vector3(
            chunkSize * 1.2f,
            (heightMax - heightMin) * 1.2f,
            chunkSize * 1.2f
        );

        // Determine grass instances (adjustable based on distance)
        float densityMultiplier = Mathf.Lerp(1.0f, 0.2f,
            distanceFromCamera / (maxRenderDistance / chunkSize));

        int instancesPerAxis = Mathf.CeilToInt(chunkSize * densityMultiplier);
        int totalInstances = instancesPerAxis * instancesPerAxis;

        // Create and fill position buffer
        Vector4[] positions = new Vector4[totalInstances];
        int instanceIndex = 0;

        for (int x = 0; x < instancesPerAxis; x++)
        {
            for (int z = 0; z < instancesPerAxis; z++)
            {
                // Calculate world position with randomization
                float posX = worldX + (x / (float)instancesPerAxis) * chunkSize + Random.Range(-density, density);
                float posZ = worldZ + (z / (float)instancesPerAxis) * chunkSize + Random.Range(-density, density);

                // Sample terrain height
                float posY = terrain.SampleHeight(new Vector3(posX, 0, posZ)) + terrainPosition.y + heightOffset;

                // Store position
                positions[instanceIndex] = new Vector4(posX, posY, posZ, 1);
                instanceIndex++;
            }
        }

        // Create chunk
        GrassChunk chunk = new GrassChunk();
        chunk.centerPosition = centerPos;
        chunk.bounds = new Bounds(centerPos, boundsSize);
        chunk.instanceCount = totalInstances;

        // Create position buffer
        chunk.positionBuffer = new ComputeBuffer(totalInstances, sizeof(float) * 4);
        chunk.positionBuffer.SetData(positions);

        // Create args buffer
        chunk.argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)totalInstances, 0, 0, 0 };
        chunk.argsBuffer.SetData(args);

        // Store chunk
        chunks[chunkCoord] = chunk;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw chunk bounds
        foreach (var kvp in chunks)
        {
            Vector2Int coord = kvp.Key;
            GrassChunk chunk = kvp.Value;

            if (chunk.positionBuffer != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(chunk.bounds.center, chunk.bounds.size);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(chunk.bounds.center,
                    $"Chunk {coord}: {chunk.instanceCount}");
#endif
            }
        }

        // Draw camera frustum
        if (mainCamera != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * 10);
        }
    }

    void OnDestroy()
    {
        foreach (var chunk in chunks.Values)
        {
            chunk.Release();
        }

        chunks.Clear();
    }
}