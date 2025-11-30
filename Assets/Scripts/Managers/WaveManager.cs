using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum WaveType
{
    Normal,
    Break,
    MiniBoss,
    Power,
    Boss,
    Spike
}

[System.Serializable]
public class Wave
{
    public string waveName = "Wave";
    public WaveType waveType = WaveType.Normal;

    [Tooltip("How many enemies to spawn in this wave (ignored for Break waves).")]
    public int enemyCount = 5;

    [Tooltip("Delay between each enemy spawn in this wave.")]
    public float spawnInterval = 0.5f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<Wave> waves = new List<Wave>();

    [Tooltip("Delay between the end of one wave and the start of the next (except Break waves, which use BreakWaveDuration).")]
    public float timeBetweenWaves = 3f;

    [Tooltip("How long a Break wave lasts (no enemies, just rest).")]
    public float breakWaveDuration = 4f;

    [Header("References")]
    [SerializeField] private EnemyHordeSpawner spawner;

    private int currentWaveIndex = -1;
    private int enemiesAlive = 0;
    private bool waveActive = false;

    [Header("Player Scaling")]
    [SerializeField] private PlayerXP playerXP;

    // How much enemies scale with each wave
    [SerializeField] private float baseHealthPerWave = 0.15f;
    [SerializeField] private float baseDamagePerWave = 0.10f;
    [SerializeField] private float baseSpeedPerWave = 0.05f;

    // How much enemies scale with each player level above 1
    [SerializeField] private float healthPerPlayerLevel = 0.08f;
    [SerializeField] private float damagePerPlayerLevel = 0.06f;
    [SerializeField] private float speedPerPlayerLevel = 0.04f;


    private void Start()
    {
        if (spawner == null)
        {
            Debug.LogError("[WaveManager] No EnemyHordeSpawner assigned!");
            return;
        }

        if (playerXP == null)
            playerXP = FindObjectOfType<PlayerXP>();

        StartCoroutine(StartWaveRoutine());
    }

    private IEnumerator StartWaveRoutine()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("[WaveManager] ALL WAVES COMPLETE!");
            yield break;
        }

        Wave wave = waves[currentWaveIndex];
        Debug.Log($"[WaveManager] Preparing {wave.waveName} ({wave.waveType})");

        if (wave.waveType == WaveType.Break)
        {
            waveActive = false;
            Debug.Log("[WaveManager] Break wave — no enemies, just chill.");
            yield return new WaitForSeconds(breakWaveDuration);
            Debug.Log("[WaveManager] Break over. Starting next wave...");
            StartCoroutine(StartWaveRoutine());
            yield break;
        }

        waveActive = true;
        enemiesAlive = 0;

        Debug.Log($"[WaveManager] Starting {wave.waveName}. Spawning {wave.enemyCount} enemies.");

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject enemy = spawner.SpawnEnemyFromWave();
            if (enemy != null)
            {
                enemiesAlive++;

                ApplyWaveModifiersToEnemy(enemy, wave);

                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.OnEnemyDied += HandleEnemyDied;
                }
            }

            yield return new WaitForSeconds(wave.spawnInterval);
        }

        Debug.Log("[WaveManager] Finished spawning for this wave.");
    }

    private void ApplyWaveModifiersToEnemy(GameObject enemy, Wave wave)
    {
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        EnemyAttack attack = enemy.GetComponent<EnemyAttack>();
        EnemyMoveAI moveAI = enemy.GetComponent<EnemyMoveAI>();

        int playerLevel = playerXP != null ? playerXP.level : 1;

        // --------- BASE MULTIPLIERS ---------
        float healthMultiplier = 1f;
        float damageMultiplier = 1f;
        float speedMultiplier = 1f;

        // Scale by wave index
        healthMultiplier += currentWaveIndex * baseHealthPerWave;
        damageMultiplier += currentWaveIndex * baseDamagePerWave;
        speedMultiplier += currentWaveIndex * baseSpeedPerWave;

        // Scale by player level
        int extraLevels = Mathf.Max(0, playerLevel - 1);
        healthMultiplier += extraLevels * healthPerPlayerLevel;
        damageMultiplier += extraLevels * damagePerPlayerLevel;
        speedMultiplier += extraLevels * speedPerPlayerLevel;

        // --------- WAVE TYPE ADJUSTMENTS ---------
        switch (wave.waveType)
        {
            case WaveType.Normal:
                break;

            case WaveType.Power:
                healthMultiplier *= 0.5f;   
                damageMultiplier *= 0.7f;  
                break;

            case WaveType.MiniBoss:
                healthMultiplier *= 3f;
                damageMultiplier *= 2f;
                speedMultiplier *= 0.9f; 
                enemy.transform.localScale *= 1.5f;
                break;

            case WaveType.Boss:
                healthMultiplier *= 5f;
                damageMultiplier *= 3f;
                speedMultiplier *= 1.1f;
                enemy.transform.localScale *= 2f;
                break;
        }

        if (health != null)
        {
            health.maxHealth = Mathf.Max(1, Mathf.RoundToInt(health.maxHealth * healthMultiplier));
        }

        if (attack != null)
        {
            attack.damage = Mathf.Max(1, Mathf.RoundToInt(attack.damage * damageMultiplier));
        }

        if (moveAI != null)
        {
            moveAI.chaseSpeed = moveAI.chaseSpeed * speedMultiplier;
        }
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
        Debug.Log($"[WaveManager] {waves[currentWaveIndex].waveName} complete! All enemies dead.");

        StartCoroutine(WaitAndStartNextWave());
    }

    private IEnumerator WaitAndStartNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartCoroutine(StartWaveRoutine());
    }
}
