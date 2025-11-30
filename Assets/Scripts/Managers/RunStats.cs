using UnityEngine;

public class RunStats : MonoBehaviour
{
    public static RunStats Instance { get; private set; }

    [Header("Core Run Stats")]
    [Tooltip("How many waves the player fully cleared this run.")]
    public int wavesCleared;

    [Tooltip("How many enemies were defeated this run.")]
    public int enemiesDefeated;

    [Tooltip("Total damage dealt by the player this run.")]
    public int damageDealt;

    [Tooltip("Total damage taken by the player this run.")]
    public int damageTaken;

    private float runStartTime;
    private float runEndTime;
    private bool runEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetStats();
    }

    public void ResetStats()
    {
        wavesCleared = 0;
        enemiesDefeated = 0;
        damageDealt = 0;
        damageTaken = 0;

        runStartTime = Time.time;
        runEndTime = 0f;
        runEnded = false;
    }

    public void EndRun()
    {
        if (runEnded) return;

        runEnded = true;
        runEndTime = Time.time;
    }

    public float GetRunDurationSeconds()
    {
        if (!runEnded)
            return Time.time - runStartTime;

        return runEndTime - runStartTime;
    }

    public void RegisterWaveCleared()
    {
        wavesCleared++;
    }

    public void RegisterEnemyDefeated()
    {
        enemiesDefeated++;
    }

    public void RegisterDamageDealt(int amount)
    {
        if (amount > 0)
            damageDealt += amount;
    }

    public void RegisterDamageTaken(int amount)
    {
        if (amount > 0)
            damageTaken += amount;
    }
}
