using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("How many enemies to spawn in this first test wave.")]
    [SerializeField] private int enemiesToSpawn = 5;

    [Tooltip("Delay between each enemy spawn within the wave.")]
    [SerializeField] private float spawnInterval = 0.5f;

    [Header("References")]
    [SerializeField] private EnemyHordeSpawner spawner; 

    private int enemiesAlive = 0;
    private bool waveActive = false;

    private void Start()
    {
        StartCoroutine(StartWaveRoutine());
    }

    private IEnumerator StartWaveRoutine()
    {
        if (waveActive)
        {
            Debug.LogWarning("Tried to start a wave while one is already active.");
            yield break;
        }

        if (spawner == null)
        {
            Debug.LogError("WaveManager has no EnemyHordeSpawner reference!");
            yield break;
        }

        waveActive = true;
        enemiesAlive = 0;

        Debug.Log("[WaveManager] Starting wave. Spawning " + enemiesToSpawn + " enemies.");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject enemy = spawner.SpawnEnemyFromWave();

            if (enemy != null)
            {
                enemiesAlive++;

                var health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.OnEnemyDied += HandleEnemyDied;
                }
                else
                {
                    Debug.LogWarning("Spawned enemy has no EnemyHealth component!");
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log("[WaveManager] All enemies for this wave have been spawned.");
    }

    private void HandleEnemyDied(EnemyHealth health)
    {
        health.OnEnemyDied -= HandleEnemyDied;

        enemiesAlive--;
        if (waveActive && enemiesAlive <= 0)
        {
            EndWave();
        }
    }

    private void EndWave()
    {
        waveActive = false;
        Debug.Log("[WaveManager] Wave complete! All enemies are dead.");
    }
}
