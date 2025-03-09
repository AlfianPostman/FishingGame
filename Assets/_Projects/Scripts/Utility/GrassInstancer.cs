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
    [Range(0.1f, 10f)]
    public float density = 2f;
    [Tooltip("Scales the number of grass instances")]
    [Range(0.1f, 5f)]
    public float densityMultiplier = 1f;
    public float heightOffset = 0.1f;
    [Tooltip("When true, updates grass when density changes")]
    public bool dynamicDensityUpdate = true;

    [Header("LOD and Culling Settings")]
    public float maxRenderDistance = 100f;
    [Tooltip("Extra distance beyond camera frustum to keep chunks loaded")]
    [Range(0f, 50f)]
    public float frustumCullingPadding = 10f;
    [Tooltip("Draw frustum culling boundaries for debugging")]
    public bool showFrustumDebug = true;

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
    private float lastDensityMultiplier;

    // Frustum culling
    private Plane[] cameraFrustumPlanes = new Plane[6];
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private float frustumCheckTimer = 0f;
    private const float FRUSTUM_UPDATE_INTERVAL = 0.1f; // Update frustum every 0.1 seconds
    private HashSet<Vector2Int> visibleChunks = new HashSet<Vector2Int>();

    // Debug options
    public bool debugMode = true;

    private class GrassChunk
    {
        public ComputeBuffer positionBuffer;
        public ComputeBuffer argsBuffer;
        public Vector3 centerPosition;
        public Bounds bounds;
        public int instanceCount;
        public float densityWhenCreated;
        public bool isVisible = true;

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
        lastDensityMultiplier = densityMultiplier;

        // Initialize camera data
        lastCameraPosition = mainCamera.transform.position;
        lastCameraRotation = mainCamera.transform.rotation;

        Debug.Log($"Terrain size: {terrainData.size}, Position: {terrainPosition}");
        Debug.Log($"Total chunks needed: {chunksPerTerrainAxis} x {chunksPerTerrainAxis}");
    }

    void Start()
    {
        propertyBlock = new MaterialPropertyBlock();

        // Initial frustum calculation
        CalculateFrustumPlanes();
    }

    void Update()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        Vector2Int currentCameraChunk = WorldPosToChunkCoord(cameraPos);

        // Update frustum planes only when needed
        frustumCheckTimer += Time.deltaTime;
        bool cameraMovedSignificantly = Vector3.Distance(lastCameraPosition, cameraPos) > 1f ||
                                       Quaternion.Angle(lastCameraRotation, mainCamera.transform.rotation) > 5f;

        if (frustumCheckTimer >= FRUSTUM_UPDATE_INTERVAL || cameraMovedSignificantly)
        {
            CalculateFrustumPlanes();
            UpdateVisibleChunks();

            lastCameraPosition = cameraPos;
            lastCameraRotation = mainCamera.transform.rotation;
            frustumCheckTimer = 0f;
        }

        // Check if density has changed and we need to rebuild
        bool densityChanged = dynamicDensityUpdate && !Mathf.Approximately(lastDensityMultiplier, densityMultiplier);

        if (debugMode && densityChanged)
        {
            Debug.Log($"Density changed from {lastDensityMultiplier} to {densityMultiplier}, rebuilding chunks");
        }

        // Rebuild chunks if camera moved to a different chunk or density changed
        if (currentCameraChunk != lastCameraChunk || densityChanged)
        {
            if (debugMode && currentCameraChunk != lastCameraChunk)
            {
                Debug.Log($"Camera moved from chunk {lastCameraChunk} to {currentCameraChunk}");
            }

            // Release all chunks if density changed
            if (densityChanged)
            {
                foreach (var chunk in chunks.Values)
                {
                    chunk.Release();
                }
                chunks.Clear();
                lastDensityMultiplier = densityMultiplier;
            }
            // Otherwise, just release distant chunks
            else
            {
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

            // Update visibility after creating/removing chunks
            UpdateVisibleChunks();
        }

        // Draw all visible chunks
        foreach (var kvp in chunks)
        {
            Vector2Int coord = kvp.Key;
            GrassChunk chunk = kvp.Value;

            // Only draw chunks that are in the visible set
            if (chunk.positionBuffer != null && chunk.argsBuffer != null && visibleChunks.Contains(coord))
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

    private void CalculateFrustumPlanes()
    {
        // Calculate camera frustum planes
        GeometryUtility.CalculateFrustumPlanes(mainCamera, cameraFrustumPlanes);
    }

    private void UpdateVisibleChunks()
    {
        // Clear previous visible chunks
        visibleChunks.Clear();

        foreach (var kvp in chunks)
        {
            Vector2Int coord = kvp.Key;
            GrassChunk chunk = kvp.Value;

            if (chunk.positionBuffer == null) continue;

            // Create a slightly expanded bounds for smoother transitions
            Bounds expandedBounds = new Bounds(chunk.bounds.center, chunk.bounds.size);
            expandedBounds.Expand(frustumCullingPadding);

            // Check if expanded bounds intersect with camera frustum
            if (GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, expandedBounds))
            {
                visibleChunks.Add(coord);
                chunk.isVisible = true;
            }
            else
            {
                chunk.isVisible = false;
            }
        }

        if (debugMode)
        {
            Debug.Log($"Visible chunks: {visibleChunks.Count} of {chunks.Count} total");
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

        // Determine grass instances based on distance and densityMultiplier
        //float distanceFactor = Mathf.Lerp(1.0f, 0.2f,
        //    distanceFromCamera / (maxRenderDistance / chunkSize));

        //// Apply the density multiplier to control overall grass density
        //float effectiveDensity = distanceFactor * densityMultiplier;

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
        chunk.densityWhenCreated = densityMultiplier;

        // Create position buffer
        chunk.positionBuffer = new ComputeBuffer(totalInstances, sizeof(float) * 4);
        chunk.positionBuffer.SetData(positions);

        // Create args buffer
        chunk.argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)totalInstances, 0, 0, 0 };
        chunk.argsBuffer.SetData(args);

        // Store chunk
        chunks[chunkCoord] = chunk;

        // Check initial visibility
        Bounds expandedBounds = new Bounds(chunk.bounds.center, chunk.bounds.size);
        expandedBounds.Expand(frustumCullingPadding);
        if (GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, expandedBounds))
        {
            visibleChunks.Add(chunkCoord);
            chunk.isVisible = true;
        }
    }

    // Method to force regenerate all chunks
    public void RegenerateAllChunks()
    {
        foreach (var chunk in chunks.Values)
        {
            chunk.Release();
        }
        chunks.Clear();
        visibleChunks.Clear();

        lastCameraChunk = new Vector2Int(-999, -999);
        lastDensityMultiplier = densityMultiplier;

        Update();
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showFrustumDebug) return;

        // Draw all chunk bounds
        foreach (var kvp in chunks)
        {
            Vector2Int coord = kvp.Key;
            GrassChunk chunk = kvp.Value;

            if (chunk.positionBuffer != null)
            {
                // Visible chunks are green, invisible chunks are red
                Gizmos.color = visibleChunks.Contains(coord) ? Color.green : new Color(1, 0, 0, 0.3f);
                Gizmos.DrawWireCube(chunk.bounds.center, chunk.bounds.size);

#if UNITY_EDITOR
                if (visibleChunks.Contains(coord))
                {
                    UnityEditor.Handles.Label(chunk.bounds.center,
                        $"Chunk {coord}: {chunk.instanceCount}");
                }
#endif
            }
        }

        // Draw camera frustum
        if (mainCamera != null && showFrustumDebug)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * 10);

            // Draw camera frustum planes
            DrawFrustum(mainCamera, Color.cyan);
        }
    }

    private void DrawFrustum(Camera camera, Color color)
    {
#if UNITY_EDITOR
        if (camera == null) return;

        // Get camera frustum corners at the far clip plane
        Vector3[] nearCorners = new Vector3[4];
        Vector3[] farCorners = new Vector3[4];

        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, nearCorners);
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, farCorners);

        // Convert corners to world space
        for (int i = 0; i < 4; i++)
        {
            nearCorners[i] = camera.transform.TransformPoint(nearCorners[i]);
            farCorners[i] = camera.transform.TransformPoint(farCorners[i]);
        }

        // Draw near plane
        Gizmos.color = color;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(nearCorners[i], nearCorners[(i + 1) % 4]);
        }

        // Draw far plane
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(farCorners[i], farCorners[(i + 1) % 4]);
        }

        // Draw connecting lines
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(nearCorners[i], farCorners[i]);
        }
#endif
    }

    void OnDestroy()
    {
        foreach (var chunk in chunks.Values)
        {
            chunk.Release();
        }

        chunks.Clear();
        visibleChunks.Clear();
    }
}