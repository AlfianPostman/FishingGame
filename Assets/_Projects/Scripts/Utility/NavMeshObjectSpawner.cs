using UnityEngine;
using UnityEngine.AI; // For working with NavMesh

public class NavMeshObjectSpawner : MonoBehaviour
{
    public GameObject objectToSpawn; // The prefab to spawn
    public int numberOfObjects; // Number of objects to spawn
    public float spawnRadius = 20f; // Radius around the spawner to find NavMesh positions
    public LayerMask navMeshMask; // Optional: Layer mask for NavMesh (if needed)

    public void SpawnObjects(int amount)
    {
        int spawnedCount = 0;

        while (spawnedCount < amount)
        {
            // Generate a random position within the spawn radius
            Vector3 randomPoint = GetRandomPoint(transform.position, spawnRadius);

            // Check if the random point is valid on the NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                // Instantiate the object at the valid position
                Instantiate(objectToSpawn, hit.position, Quaternion.identity);
                spawnedCount++;
            }
        }
    }

    Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        // Generate a random point within a circle on the XZ plane
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);
    }
}
