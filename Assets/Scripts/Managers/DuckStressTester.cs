using UnityEngine;

public class DuckStressTester : MonoBehaviour
{
    public GameObject duckPrefab;
    public Transform spawnCenter;

    public float spawnRadius = 5f;

    void Update()
    {
        // Spawn 5 ducks (key: 5)
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SpawnDucks(5);
        }

        // Spawn 10 ducks (key: 6)
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SpawnDucks(10);
        }

        // Spawn 20 ducks (key: 7)
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SpawnDucks(20);
        }
    }

    void SpawnDucks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 ringPos = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = new Vector3(ringPos.x, 0.5f, ringPos.y) + spawnCenter.position;

            Instantiate(duckPrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log($"Spawned {count} ducks. Total: {FindObjectsOfType<DuckEnemy>().Length}");
    }
}
