using UnityEngine;

public class EnemyHordeSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;        
    public GameObject enemyPrefab;  

    [Header("Spawn Area (around player)")]
    public float minSpawnRadius = 8f;
    public float maxSpawnRadius = 14f;

    [Header("Spawn Timing")]
    public float startInterval = 2f;        
    public float minInterval = 0.5f;        
    public float timeToMaxDifficulty = 90f; 

    [Header("Limits")]
    public int maxAliveEnemies = 50;

    float elapsedTime;
    float spawnTimer;
    int aliveEnemies;

    public GameObject SpawnEnemyFromWave()
    {
        if (enemyPrefab == null || player == null)
        {
            Debug.LogWarning("EnemyHordeSpawner is missing player or enemyPrefab reference.");
            return null;
        }

        Vector3 playerPos = player.position;

        Vector2 dir2D = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector3 offset = new Vector3(dir2D.x, 0f, dir2D.y) * distance;
        Vector3 spawnPos = playerPos + offset;

        spawnPos.y = playerPos.y + 0.5f;

        Vector3 toPlayer = (playerPos - spawnPos);
        toPlayer.y = 0f;
        Quaternion lookRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, lookRot);

        aliveEnemies++;

        var duck = enemy.GetComponent<DuckEnemy>();
        if (duck != null)
        {
            duck.spawner = this;
        }

        var moveAI = enemy.GetComponent<EnemyMoveAI>();
        if (moveAI != null)
        {
            moveAI.player = player;
            moveAI.moveByAgent = true;
        }

        return enemy;
    }

    public void NotifyEnemyDied()
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }
}
