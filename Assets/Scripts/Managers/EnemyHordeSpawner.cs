using UnityEngine;

public class EnemyHordeSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Tooltip("Default/basic duck used in normal waves.")]
    public GameObject basicDuckPrefab;

    [Tooltip("Fast duck prefab for FastDuck waves or weighted spawns.")]
    public GameObject fastDuckPrefab;

    [Tooltip("Tank duck prefab for heavier enemies.")]
    public GameObject tankDuckPrefab;

    [Header("Spawn Area (around player)")]
    public float minSpawnRadius = 8f;
    public float maxSpawnRadius = 14f;

    [Header("Spawn Timing")]
    public float startInterval = 2f;
    public float minInterval = 0.5f;
    public float timeToMaxDifficulty = 90f;

    [Header("Limits")]
    public int maxAliveEnemies = 50;

    [Header("Normal Wave Composition")]
    [Range(0f, 1f)]
    public float fastDuckChance = 0.25f;

    [Range(0f, 1f)]
    public float tankDuckChance = 0.15f;

    float elapsedTime;
    float spawnTimer;
    int aliveEnemies;


    public GameObject SpawnEnemyFromWave()
    {
        if (player == null)
        {
            Debug.LogWarning("EnemyHordeSpawner is missing player reference.");
            return null;
        }

        if (aliveEnemies >= maxAliveEnemies)
        {
            return null;
        }

        GameObject prefabToSpawn = ChoosePrefabForNormalWave();
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("No enemy prefab assigned on EnemyHordeSpawner.");
            return null;
        }

        Vector3 spawnPos;
        Quaternion lookRot;
        GetSpawnTransform(out spawnPos, out lookRot);

        return SpawnAndInitialize(prefabToSpawn, spawnPos, lookRot);
    }

    public GameObject SpawnFastDuck()
    {
        if (player == null)
        {
            Debug.LogWarning("EnemyHordeSpawner is missing player reference.");
            return null;
        }

        if (aliveEnemies >= maxAliveEnemies)
        {
            return null;
        }

        GameObject prefabToSpawn = fastDuckPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("Fast duck prefab not set on EnemyHordeSpawner. Falling back to normal spawn.");
            return SpawnEnemyFromWave();
        }

        Vector3 spawnPos;
        Quaternion lookRot;
        GetSpawnTransform(out spawnPos, out lookRot);

        return SpawnAndInitialize(prefabToSpawn, spawnPos, lookRot);
    }

    // --------- Helper Methods ---------

    GameObject ChoosePrefabForNormalWave()
    {
        if (basicDuckPrefab == null && fastDuckPrefab == null && tankDuckPrefab == null)
            return null;

        if (basicDuckPrefab != null && fastDuckPrefab == null && tankDuckPrefab == null)
            return basicDuckPrefab;

        float fastChance = Mathf.Clamp01(fastDuckChance);
        float tankChance = Mathf.Clamp01(tankDuckChance);
        float basicChance = Mathf.Clamp01(1f - fastChance - tankChance);

        float roll = Random.value;

        if (roll < fastChance && fastDuckPrefab != null)
            return fastDuckPrefab;

        if (roll < fastChance + tankChance && tankDuckPrefab != null)
            return tankDuckPrefab;

        if (basicDuckPrefab != null)
            return basicDuckPrefab;

        if (fastDuckPrefab != null) return fastDuckPrefab;
        if (tankDuckPrefab != null) return tankDuckPrefab;

        return null;
    }

    void GetSpawnTransform(out Vector3 spawnPos, out Quaternion lookRot)
    {
        Vector3 playerPos = player.position;

        Vector2 dir2D = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        Vector3 offset = new Vector3(dir2D.x, 0f, dir2D.y) * distance;
        spawnPos = playerPos + offset;

        spawnPos.y = playerPos.y + 0.5f;

        Vector3 toPlayer = (playerPos - spawnPos);
        toPlayer.y = 0f;
        lookRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
    }

    GameObject SpawnAndInitialize(GameObject prefab, Vector3 spawnPos, Quaternion lookRot)
    {
        GameObject enemy = Instantiate(prefab, spawnPos, lookRot);

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
