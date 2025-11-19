using UnityEngine;

public class EnemyHordeSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform center;        // Spawn center (SpawnCenter object)
    public GameObject enemyPrefab;  // Your duck prefab

    [Header("Spawn Area (around center)")]
    public float minSpawnRadius = 8f;
    public float maxSpawnRadius = 14f;

    [Header("Spawn Timing")]
    public float startInterval = 2f;        // seconds between spawns at start
    public float minInterval = 0.5f;        // fastest spawn interval
    public float timeToMaxDifficulty = 90f; // seconds until minInterval

    [Header("Limits")]
    public int maxAliveEnemies = 50;

    float elapsedTime;
    float spawnTimer;
    int aliveEnemies;

    void Update()
    {
        if (enemyPrefab == null) return;

        elapsedTime += Time.deltaTime;

        // difficulty 0 → 1 over time
        float t = Mathf.Clamp01(elapsedTime / timeToMaxDifficulty);
        float currentInterval = Mathf.Lerp(startInterval, minInterval, t);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && aliveEnemies < maxAliveEnemies)
        {
            SpawnEnemy();
            spawnTimer = currentInterval;
        }
    }

    void SpawnEnemy()
    {
        // If no center assigned, default to world origin
        Vector3 centerPos = center != null ? center.position : Vector3.zero;

        // Random direction on XZ plane
        Vector2 dir2D = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector3 offset = new Vector3(dir2D.x, 0f, dir2D.y) * distance;
        Vector3 spawnPos = centerPos + offset;

        // Put them just above the ground so they drop nicely
        spawnPos.y = centerPos.y + 0.5f;

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        aliveEnemies++;

        // Optional: future-proof for when enemies can die
        var duck = enemy.GetComponent<DuckEnemy>();
        if (duck != null)
        {
            duck.spawner = this;
        }
    }

    // Call this later when you add death logic
    public void NotifyEnemyDied()
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }
}
