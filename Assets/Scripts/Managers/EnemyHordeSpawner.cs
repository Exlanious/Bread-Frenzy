using UnityEngine;

public class EnemyHordeSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;        // Player transform
    public GameObject enemyPrefab;  // Your duck prefab

    [Header("Spawn Area (around player)")]
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
        // need both a prefab and a player to do anything
        if (enemyPrefab == null || player == null) return;

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
        Vector3 playerPos = player.position;

        // Random direction on XZ plane
        Vector2 dir2D = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector3 offset = new Vector3(dir2D.x, 0f, dir2D.y) * distance;
        Vector3 spawnPos = playerPos + offset;

        // Put them just above the ground so they drop nicely
        spawnPos.y = playerPos.y + 0.5f;

        // Face the player
        Vector3 toPlayer = (playerPos - spawnPos);
        toPlayer.y = 0f; // keep them level
        Quaternion lookRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, lookRot);
        aliveEnemies++;

        // Optional: future-proof for when enemies can die
        var duck = enemy.GetComponent<DuckEnemy>();
        if (duck != null)
        {
            duck.spawner = this;
        }

        var moveAI = enemy.GetComponent<EnemyMoveAI>();
        if (moveAI != null)
        {
            moveAI.player = player; // <-- this is critical
            moveAI.moveByAgent = true; // enable NavMesh chasing
        }
    }

    // Call this later when you add death logic
    public void NotifyEnemyDied()
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }
}
