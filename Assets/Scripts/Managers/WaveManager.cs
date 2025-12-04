using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Types of waves we support
public enum WaveType
{
    Normal,
    Break,
    MiniBoss,
    Power,
    Boss
}

[System.Serializable]
public class Wave
{
    public string waveName = "Wave";
    public WaveType waveType = WaveType.Normal;
    public int enemyCount = 5;
    public float spawnInterval = 0.5f;
}

public class WaveManager : MonoBehaviour
{
    public event Action<Wave, int> OnWaveStarted;

    [Header("Endless Wave Settings")]
    [Tooltip("Base enemy count for the very first combat wave.")]
    public int startingEnemyCount = 3;

    [Tooltip("How much the base enemy count grows each wave (multiplier).")]
    public float enemyCountGrowth = 1.25f;

    [Tooltip("Starting spawn interval between enemies in early waves.")]
    public float startingSpawnInterval = 0.8f;

    [Tooltip("Minimum spawn interval as waves get faster.")]
    public float minSpawnInterval = 0.15f;

    [Tooltip("Each wave, spawn interval *= this (e.g. 0.95 = 5% faster per wave).")]
    public float spawnIntervalDecay = 0.95f;

    [Header("Special Wave Cadence")]
    [Tooltip("Every N waves, spawn a Break wave. Set <= 0 to disable.")]
    public int breakEvery = 5;

    [Tooltip("Every N waves, spawn a Mini-Boss wave. Set <= 0 to disable.")]
    public int miniBossEvery = 7;

    [Tooltip("Every N waves, spawn a Boss wave. Set <= 0 to disable.")]
    public int bossEvery = 10;

    [Tooltip("Every N waves, spawn a Power wave (if not overridden by boss/miniboss/break). Set <= 0 to disable.")]
    public int powerEvery = 4;

    [Header("Timing")]
    public float timeBetweenWaves = 3f;
    public float breakWaveDuration = 4f;

    [Header("Refs")]
    [SerializeField] private EnemyHordeSpawner spawner;
    [SerializeField] private PlayerXP playerXP;

    [Header("Break Wave Effects")]
    [Tooltip("How much HP the player heals at the start of a Break wave.")]
    public int breakWaveHealAmount = 1;

    [SerializeField] private PlayerHealth playerHealth;

    [Header("Scaling vs Wave & Player")]
    [Tooltip("Extra enemy max health per wave (e.g. 0.15 = +15% per wave).")]
    public float healthPerWave = 0.15f;

    [Tooltip("Extra enemy max health per player level above 1.")]
    public float healthPerLevel = 0.10f;

    [Tooltip("Extra enemy damage per wave.")]
    public float damagePerWave = 0.04f;

    [Tooltip("Extra enemy damage per player level above 1.")]
    public float damagePerLevel = 0.03f;

    [Tooltip("Extra enemy move speed per wave.")]
    public float speedPerWave = 0.05f;

    [Tooltip("Extra enemy move speed per player level above 1.")]
    public float speedPerLevel = 0.04f;
    [Header("XP Scaling")]
    [Tooltip("Extra enemy XP per wave (multiplier per wave).")]
    public float xpPerWave = 0.20f;   

    [Tooltip("Extra enemy XP per player level above 1.")]
    public float xpPerLevel = 0.10f;  

    private int currentWaveNumber = 0;   
    private int enemiesAlive = 0;
    private bool waveActive = false;

    private int currentBaseEnemyCount;
    private float currentSpawnInterval;

    private Wave currentWave;
    private bool isShuttingDown = false;

    private void Start()
    {
        if (spawner == null)
        {
            Debug.LogError("[WaveManager] No EnemyHordeSpawner assigned!");
            return;
        }

        if (playerXP == null)
        {
            playerXP = FindObjectOfType<PlayerXP>();
            if (playerXP == null)
            {
                Debug.LogWarning("[WaveManager] No PlayerXP found. Scaling will ignore player level.");
            }
        }

        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogWarning("[WaveManager] No PlayerHealth found. Break wave healing will be disabled.");
            }
        }

        currentBaseEnemyCount = startingEnemyCount;
        currentSpawnInterval = startingSpawnInterval;

        StartCoroutine(StartNextWave());
    }

    private IEnumerator StartNextWave()
    {
        currentWaveNumber++;

        currentWave = GenerateWave(currentWaveNumber);

        OnWaveStarted?.Invoke(currentWave, currentWaveNumber);

        Debug.Log($"[WaveManager] Preparing {currentWave.waveName} (Wave {currentWaveNumber}, {currentWave.waveType})");

        if (currentWave.waveType == WaveType.Break)
        {
            waveActive = false;
            enemiesAlive = 0;

            Debug.Log("[WaveManager] Break wave � no enemies. Player can breathe.");

            if (playerHealth != null && breakWaveHealAmount > 0)
            {
                playerHealth.Heal(breakWaveHealAmount);
                Debug.Log($"[WaveManager] Break wave heal: +{breakWaveHealAmount} HP");
            }

            yield return new WaitForSeconds(breakWaveDuration);

            Debug.Log("[WaveManager] Break over. Next wave soon...");
            yield return new WaitForSeconds(timeBetweenWaves);
            StartCoroutine(StartNextWave());
            yield break;
        }


        waveActive = true;
        enemiesAlive = 0;

        Debug.Log($"[WaveManager] Starting {currentWave.waveName}. Spawning {currentWave.enemyCount} enemies.");

        for (int i = 0; i < currentWave.enemyCount; i++)
        {
            GameObject enemy = spawner.SpawnEnemyFromWave();
            if (enemy != null)
            {
                enemiesAlive++;

                ApplyWaveModifiersToEnemy(enemy, currentWave);

                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.OnEnemyDied += HandleEnemyDied;
                }
            }

            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        Debug.Log("[WaveManager] Finished spawning for this wave.");
    }

    private Wave GenerateWave(int waveNumber)
    {
        Wave wave = new Wave();

        bool isBoss = bossEvery > 0 && waveNumber % bossEvery == 0;
        bool isMiniBoss = miniBossEvery > 0 && waveNumber % miniBossEvery == 0;
        bool isBreak = breakEvery > 0 && waveNumber % breakEvery == 0;
        bool isPower = powerEvery > 0 && waveNumber % powerEvery == 0;

        if (isBoss)
        {
            wave.waveType = WaveType.Boss;
        }
        else if (isMiniBoss)
        {
            wave.waveType = WaveType.MiniBoss;
        }
        else if (isBreak)
        {
            wave.waveType = WaveType.Break;
        }
        else if (isPower)
        {
            wave.waveType = WaveType.Power;
        }
        else
        {
            wave.waveType = WaveType.Normal;
        }

        switch (wave.waveType)
        {
            case WaveType.Break:
                wave.waveName = "Breather";
                wave.enemyCount = 0;
                wave.spawnInterval = 0f;
                break;

            case WaveType.MiniBoss:
                wave.waveName = $"Mini-Boss {waveNumber}";
                wave.enemyCount = 1;
                wave.spawnInterval = 0.1f;
                break;

            case WaveType.Boss:
                wave.waveName = $"Boss Wave {waveNumber}";
                wave.enemyCount = 1;
                wave.spawnInterval = 0.1f;
                break;

            case WaveType.Power:
                wave.waveName = "Power Wave";
                int powerCount = Mathf.RoundToInt(currentBaseEnemyCount * 1.6f);
                wave.enemyCount = Mathf.Max(8, powerCount);
                wave.spawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval * 0.6f);
                break;

            case WaveType.Normal:
            default:
                wave.waveName = $"Wave {waveNumber}";

                int variation = Mathf.RoundToInt(currentBaseEnemyCount * 0.25f);
                int randomOffset = UnityEngine.Random.Range(-variation, variation + 1);
                int normalCount = currentBaseEnemyCount + randomOffset;

                wave.enemyCount = Mathf.Max(2, normalCount);
                wave.spawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval);
                break;
        }

        if (wave.waveType != WaveType.Break)
        {
            currentBaseEnemyCount = Mathf.RoundToInt(currentBaseEnemyCount * enemyCountGrowth);
            currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval * spawnIntervalDecay);
        }

        return wave;
    }

    private void ApplyWaveModifiersToEnemy(GameObject enemy, Wave wave)
    {
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        EnemyAttack attack = enemy.GetComponent<EnemyAttack>();
        EnemyMoveAI moveAI = enemy.GetComponent<EnemyMoveAI>();
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        int playerLevel = (playerXP != null) ? Mathf.Max(1, playerXP.level) : 1;
        int levelsAboveOne = Mathf.Max(0, playerLevel - 1);

        float healthMult = 1f
            + (currentWaveNumber - 1) * healthPerWave
            + levelsAboveOne * healthPerLevel;

        float damageMult = 1f
            + (currentWaveNumber - 1) * damagePerWave
            + levelsAboveOne * damagePerLevel;

        float speedMult = 1f
            + (currentWaveNumber - 1) * speedPerWave
            + levelsAboveOne * speedPerLevel;

        float xpMult = 1f
            + (currentWaveNumber - 1) * xpPerWave
            + levelsAboveOne * xpPerLevel;

        switch (wave.waveType)
        {
            case WaveType.Power:
                healthMult *= 0.5f;
                damageMult *= 0.8f;
                speedMult *= 0.9f;
                xpMult *= 1.2f;   
                break;

            case WaveType.MiniBoss:
                healthMult *= 3f;
                damageMult *= 1.8f;
                speedMult *= 0.9f;
                xpMult *= 4f;     
                enemy.transform.localScale *= 1.5f;
                break;

            case WaveType.Boss:
                healthMult *= 5f;
                damageMult *= 2.2f;
                speedMult *= 1.0f;
                xpMult *= 8f;     
                enemy.transform.localScale *= 2f;
                break;
        }

        if (health != null)
        {
            health.maxHealth = Mathf.Max(1, Mathf.RoundToInt(health.maxHealth * healthMult));

            int baseXP = Mathf.Max(1, health.xpValue);
            health.xpValue = Mathf.Max(1, Mathf.RoundToInt(baseXP * xpMult));
        }

        if (attack != null)
        {
            attack.damage = Mathf.Max(1, Mathf.RoundToInt(attack.damage * damageMult));
        }

        if (moveAI != null)
        {
            moveAI.chaseSpeed *= speedMult;
            if (agent != null) agent.speed = moveAI.chaseSpeed;
        }
    }

    private void HandleEnemyDied(EnemyHealth health)
    {
        if (isShuttingDown || !this || !isActiveAndEnabled)
            return;

        if (health != null)
        {
            health.OnEnemyDied -= HandleEnemyDied;
        }

        enemiesAlive--;

        if (RunStats.Instance != null)
        {
            RunStats.Instance.RegisterEnemyDefeated();
        }

        if (!waveActive || enemiesAlive > 0)
            return;

        EndWave();
    }

    private void EndWave()
    {
        if (isShuttingDown || !this || !isActiveAndEnabled)
            return;

        waveActive = false;
        Debug.Log($"[WaveManager] {currentWave.waveName} (Wave {currentWaveNumber}) complete! All enemies dead.");

        if (RunStats.Instance != null && currentWave != null && currentWave.waveType != WaveType.Break)
        {
            RunStats.Instance.RegisterWaveCleared();
        }

        StartCoroutine(WaitAndStartNextWave());
    }

    private IEnumerator WaitAndStartNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartCoroutine(StartNextWave());
    }

    private void OnDisable()
    {
        isShuttingDown = true;
    }

    private void OnDestroy()
    {
        isShuttingDown = true;
    }

}
