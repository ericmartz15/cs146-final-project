// PedestrianSpawner.cs
// Spawns pedestrians at random road tile positions and initializes their crossing behavior.
// Attach this to any persistent GameObject (e.g. GameManager).

using UnityEngine;
using UnityEngine.Tilemaps;

public class PedestrianSpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject pedestrianPrefab;
    [SerializeField] private Tilemap roadTilemap;
    [SerializeField] private int spawnCount = 10;
    [SerializeField] private float minDistanceFromPlayer = 3f;

    [Header("Player")]
    [SerializeField] private Transform player;

    void Start()
    {
        SpawnPedestrians();
    }

    private void SpawnPedestrians()
    {
        if (pedestrianPrefab == null || roadTilemap == null)
        {
            Debug.LogError("PedestrianSpawner: Missing prefab or tilemap!");
            return;
        }

        BoundsInt bounds = roadTilemap.cellBounds;
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = spawnCount * 20;

        while (spawned < spawnCount && attempts < maxAttempts)
        {
            attempts++;

            int randomX = Random.Range(bounds.xMin, bounds.xMax);
            int randomY = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);

            if (!roadTilemap.HasTile(randomCell)) continue;

            Vector3 worldPos = roadTilemap.GetCellCenterWorld(randomCell);

            if (player != null && Vector3.Distance(worldPos, player.position) < minDistanceFromPlayer)
                continue;

            GameObject pedestrian = Instantiate(pedestrianPrefab, worldPos, Quaternion.identity);

            // Initialize crossing behavior
            PedestrianBehavior behavior = pedestrian.GetComponent<PedestrianBehavior>();
            if (behavior != null)
                behavior.Initialize(roadTilemap);

            spawned++;
        }

        Debug.Log($"PedestrianSpawner: Spawned {spawned} pedestrians.");
    }
}