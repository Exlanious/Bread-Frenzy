using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum WaveType
{
    Normal,
    Break,
    MiniBoss,
    Power,
    Boss,
    FastDuck,
    AllRanged,  
    PanicMix     
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

    [Tooltip("Every N waves, spawn an All-Fast-Ducks wave. Set <= 0 to disable.")]
    public int fastDuckEvery = 3;

    [Tooltip("Every N waves, spawn a Mini-Boss wave. Set <= 0 to disable.")]
    public int miniBossEvery = 7;

    [Tooltip("Every N waves, spawn a Boss wave. Set <= 0 to disable.")]
    public int bossEvery = 10;

    [Tooltip("Every N waves, spawn a Power wave (if not overridden by boss/miniboss/break). Set <= 0 to disable.")]
    public int powerEvery = 4;

    [Tooltip("Every N waves, spawn an All-Ranged wave. Set <= 0 to disable.")]
    public int allRangedEvery = 6;  

    [Tooltip("Every N waves, spawn a Panic mixed wave (all enemies spawn at once). Set <= 0 to disable.")]
    public int panicMixEvery = 8;    

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

    [Header("Boss Waves")]
    [Tooltip("Prefab that will be spawned for Boss waves.")]
    public GameObject bossPrefab;

    [Tooltip("Optional spawn points for bosses. If empty, uses the spawner's transform.")]
    public Transform[] bossSpawnPoints;

    [Header("Debug Sample Waves")]
    [SerializeField] private bool enableSampleWaveHotkeys = false;

    [SerializeField] private Wave sampleNormalWave = new Wave
    {
        waveName = "Sample Normal",
        waveType = WaveType.Normal,
        enemyCount = 5,
        spawnInterval = 0.5f
    };

    [SerializeField] private Wave samplePowerWave = new Wave
    {
        waveName = "Sample Power",
        waveType = WaveType.Power,
        enemyCount = 3,
        spawnInterval = 0.6f
    };

    [SerializeField] private Wave sampleBreakWave = new Wave
    {
        waveName = "Sample Break",
        waveType = WaveType.Break,
        enemyCount = 0,
        spawnInterval = 0f
    };

    [SerializeField] private Wave sampleMiniBossWave = new Wave
    {
        waveName = "Sample MiniBoss",
        waveType = WaveType.MiniBoss,
        enemyCount = 1,
        spawnInterval = 0f
    };

    [SerializeField] private Wave sampleBossWave = new Wave
    {
        waveName = "Sample Boss",
        waveType = WaveType.Boss,
        enemyCount = 1,
        spawnInterval = 0f
    };

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

        if (bossEvery > 0 && bossPrefab == null)
        {
            Debug.LogWarning("[WaveManager] Boss waves are enabled but no bossPrefab is assigned.");
        }

        currentBaseEnemyCount = startingEnemyCount;
        currentSpawnInterval = startingSpawnInterval;

        StartCoroutine(StartNextWave());
    }

    private void Update()
    {
        if (!enableSampleWaveHotkeys || isShuttingDown)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            TriggerSampleWave(sampleNormalWave);

        else if (Input.GetKeyDown(KeyCode.Alpha2))
            TriggerSampleWave(samplePowerWave);

        else if (Input.GetKeyDown(KeyCode.Alpha3))
            TriggerSampleWave(sampleBreakWave);

        else if (Input.GetKeyDown(KeyCode.Alpha4))
            TriggerSampleWave(sampleMiniBossWave);

        else if (Input.GetKeyDown(KeyCode.Alpha5))
            TriggerSampleWave(sampleBossWave);
    }

    private void TriggerSampleWave(Wave wave)
    {
        Debug.Log($"[WaveManager] DEBUG: Triggering sample wave: {wave.waveName}");

        StopAllCoroutines();
        ClearAllEnemiesImmediate();

        waveActive = false;
        enemiesAlive = 0;

        StartCoroutine(RunWave(wave, allowAutoChainToNext: false));
    }

    private void ClearAllEnemiesImmediate()
    {
        EnemyHealth[] allEnemies = FindObjectsOfType<EnemyHealth>();
        foreach (var enemy in allEnemies)
        {
            enemy.OnEnemyDied -= HandleEnemyDied;
            Destroy(enemy.gameObject);
        }
    }

    private void JumpToWave(int waveNumber)
    {
        Debug.Log($"[WaveManager] DEBUG: Jumping to wave {waveNumber} via hotkey.");

        StopAllCoroutines();

        ClearAllEnemiesImmediate();

        waveActive = false;
        enemiesAlive = 0;

        currentBaseEnemyCount = startingEnemyCount;
        currentSpawnInterval = startingSpawnInterval;

        for (int i = 1; i < waveNumber; i++)
        {
            GenerateWave(i);
        }

        currentWaveNumber = waveNumber - 1;

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

            Debug.Log("[WaveManager] Break wave – no enemies. Player can breathe.");

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
            GameObject enemy = null;

            if (currentWave.waveType == WaveType.Boss && bossPrefab != null)
            {
                Transform spawnPoint = GetRandomBossSpawnPoint();
                enemy = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                switch (currentWave.waveType)
                {
                    case WaveType.FastDuck:
                        enemy = spawner.SpawnFastDuck();
                        break;

                    case WaveType.AllRanged:
                        enemy = spawner.SpawnRangedDuck();
                        break;

                    case WaveType.Normal:
                        enemy = spawner.SpawnBasicDuck();
                        break;

                    default:
                        enemy = spawner.SpawnEnemyFromWave();
                        break;
                }
            }

            if (enemy != null)
            {
                enemiesAlive++;

                bool isPrimaryMiniBoss = (currentWave.waveType == WaveType.MiniBoss && i == 0);
                ApplyWaveModifiersToEnemy(enemy, currentWave, isPrimaryMiniBoss);

                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.OnEnemyDied += HandleEnemyDied;
                }
            }

            if (currentWave.waveType != WaveType.PanicMix)
            {
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
        }

        Debug.Log("[WaveManager] Finished spawning for this wave.");
    }

    private Transform GetRandomBossSpawnPoint()
    {
        if (bossSpawnPoints != null && bossSpawnPoints.Length > 0)
        {
            int index = UnityEngine.Random.Range(0, bossSpawnPoints.Length);
            return bossSpawnPoints[index];
        }

        return spawner.transform;
    }

    private Wave GenerateWave(int waveNumber)
    {
        Wave wave = new Wave();

        bool isBoss      = bossEvery      > 0 && waveNumber % bossEvery      == 0;
        bool isMiniBoss  = miniBossEvery  > 0 && waveNumber % miniBossEvery  == 0;
        bool isBreak     = breakEvery     > 0 && waveNumber % breakEvery     == 0;
        bool isPower     = powerEvery     > 0 && waveNumber % powerEvery     == 0;
        bool isFastDuck  = fastDuckEvery  > 0 && waveNumber % fastDuckEvery  == 0;
        bool isAllRanged = allRangedEvery > 0 && waveNumber % allRangedEvery == 0;
        bool isPanicMix  = panicMixEvery  > 0 && waveNumber % panicMixEvery  == 0;

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
        else
        {

            List<WaveType> candidates = new List<WaveType>();

            candidates.Add(WaveType.Normal);

            if (isPower)
                candidates.Add(WaveType.Power);

            if (isFastDuck)
                candidates.Add(WaveType.FastDuck);

            if (isAllRanged)
                candidates.Add(WaveType.AllRanged);

            if (isPanicMix)
                candidates.Add(WaveType.PanicMix);

            int index = UnityEngine.Random.Range(0, candidates.Count);
            wave.waveType = candidates[index];
        }

        switch (wave.waveType)
        {
            case WaveType.Break:
                wave.waveName = "Breather";
                wave.enemyCount = 0;
                wave.spawnInterval = 0f;
                break;

            case WaveType.MiniBoss:
                wave.waveName = $"Mini-Boss + Ranged {waveNumber}";
                int miniTotal = Mathf.Max(4, currentBaseEnemyCount / 2);
                wave.enemyCount = miniTotal;
                wave.spawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval);
                break;

            case WaveType.Boss:
                wave.waveName = $"Boss Wave {waveNumber}";
                wave.enemyCount = 1;
                wave.spawnInterval = 0.1f;
                break;

            case WaveType.Power:
                wave.waveName = "Mixed Wave";
                int powerBase = Mathf.RoundToInt(currentBaseEnemyCount * 1.6f);
                int powerVar  = Mathf.RoundToInt(powerBase * 0.25f);
                int powerCount = powerBase + UnityEngine.Random.Range(-powerVar, powerVar + 1);
                wave.enemyCount = Mathf.Max(8, powerCount);

                float powerInterval = currentSpawnInterval * UnityEngine.Random.Range(0.55f, 0.75f);
                wave.spawnInterval = Mathf.Max(minSpawnInterval, powerInterval);
                break;

            case WaveType.FastDuck:
                wave.waveName = "Fast Duck Swarm";
                int fastBase = Mathf.RoundToInt(currentBaseEnemyCount * 1.2f);
                int fastVar  = Mathf.RoundToInt(fastBase * 0.25f);
                int fastCount = fastBase + UnityEngine.Random.Range(-fastVar, fastVar + 1);
                wave.enemyCount = Mathf.Max(6, fastCount);

                float fastInterval = currentSpawnInterval * UnityEngine.Random.Range(0.6f, 0.8f);
                wave.spawnInterval = Mathf.Max(minSpawnInterval, fastInterval);
                break;

            case WaveType.AllRanged:
                wave.waveName = "Sniper Ducks";
                int rangedBase = Mathf.RoundToInt(currentBaseEnemyCount * 1.1f);
                int rangedVar  = Mathf.RoundToInt(rangedBase * 0.25f);
                int rangedCount = rangedBase + UnityEngine.Random.Range(-rangedVar, rangedVar + 1);
                wave.enemyCount = Mathf.Max(4, rangedCount);

                float rangedInterval = currentSpawnInterval * UnityEngine.Random.Range(0.8f, 1.0f);
                wave.spawnInterval = Mathf.Max(minSpawnInterval, rangedInterval);
                break;

            case WaveType.PanicMix:
                wave.waveName = "Duck Ambush";
                int panicBase = Mathf.RoundToInt(currentBaseEnemyCount * 1.3f);
                int panicVar  = Mathf.RoundToInt(panicBase * 0.3f);
                int panicCount = panicBase + UnityEngine.Random.Range(-panicVar, panicVar + 1);
                wave.enemyCount = Mathf.Max(6, panicCount);

                wave.spawnInterval = minSpawnInterval;
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

    private void ApplyWaveModifiersToEnemy(GameObject enemy, Wave wave, bool isPrimaryInWave = false)
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
                if (isPrimaryInWave)
                {
                    healthMult *= 3f;
                    damageMult *= 1.8f;
                    speedMult *= 0.9f;
                    xpMult *= 4f;
                    enemy.transform.localScale *= 1.5f;
                }
                else
                {
                    healthMult *= 1.2f;
                    damageMult *= 1.2f;
                    xpMult *= 1.5f;
                }
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

    private IEnumerator RunWave(Wave wave, bool allowAutoChainToNext)
    {
        currentWave = wave;

        OnWaveStarted?.Invoke(currentWave, currentWaveNumber);

        Debug.Log($"[WaveManager] DEBUG SAMPLE — Running: {wave.waveName} ({wave.waveType})");

        if (wave.waveType == WaveType.Break)
        {
            waveActive = false;
            enemiesAlive = 0;

            if (playerHealth != null && breakWaveHealAmount > 0)
            {
                playerHealth.Heal(breakWaveHealAmount);
            }

            yield return new WaitForSeconds(breakWaveDuration);

            if (allowAutoChainToNext)
            {
                StartCoroutine(StartNextWave());
            }

            yield break;
        }

        waveActive = true;
        enemiesAlive = 0;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject enemy = null;

            if (wave.waveType == WaveType.Boss && bossPrefab != null)
            {
                Transform spawnPoint = GetRandomBossSpawnPoint();
                enemy = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                switch (wave.waveType)
                {
                    case WaveType.FastDuck:
                        enemy = spawner.SpawnFastDuck();
                        break;

                    case WaveType.AllRanged:
                        enemy = spawner.SpawnRangedDuck();
                        break;

                    case WaveType.Normal:
                        enemy = spawner.SpawnBasicDuck();
                        break;

                    default:
                        enemy = spawner.SpawnEnemyFromWave();
                        break;
                }
            }

            if (enemy != null)
            {
                enemiesAlive++;

                bool isPrimaryMiniBoss = (wave.waveType == WaveType.MiniBoss && i == 0);
                ApplyWaveModifiersToEnemy(enemy, wave, isPrimaryMiniBoss);

                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.OnEnemyDied += HandleEnemyDied;
                }
            }

            if (wave.waveType != WaveType.PanicMix)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        Debug.Log("[WaveManager] DEBUG SAMPLE — Finished spawning.");
    }
}
