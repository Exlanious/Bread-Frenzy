using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Options")]
    [Tooltip("Pause the game when the player dies.")]
    [SerializeField] private bool pauseOnGameOver = true;

    [Header("Stat Texts")]
    [SerializeField] private TMP_Text timeValueText;
    [SerializeField] private TMP_Text wavesValueText;
    [SerializeField] private TMP_Text enemiesValueText;
    [SerializeField] private TMP_Text damageValueText;

    [Header("Other UI To Hide")]
    [SerializeField] private GameObject[] uiToHideOnGameOver;

    private bool isGameOver = false;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnPlayerDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (RunStats.Instance != null)
        {
            RunStats.Instance.EndRun();
            UpdateStatsUI(RunStats.Instance);
        }

        if (uiToHideOnGameOver != null)
        {
            foreach (var ui in uiToHideOnGameOver)
            {
                if (ui != null)
                    ui.SetActive(false);
            }
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (pauseOnGameOver)
            Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        if (RunStats.Instance != null)
        {
            RunStats.Instance.ResetStats();
        }

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }


    private void UpdateStatsUI(RunStats stats)
    {
        if (stats == null)
            return;

        if (timeValueText != null)
        {
            float seconds = stats.GetRunDurationSeconds();
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int secs = totalSeconds % 60;

            timeValueText.text = $"{minutes}:{secs:00}";
        }

        if (wavesValueText != null)
        {
            wavesValueText.text = stats.wavesCleared.ToString();
        }

        if (enemiesValueText != null)
        {
            enemiesValueText.text = stats.enemiesDefeated.ToString();
        }

        if (damageValueText != null)
        {
            damageValueText.text = stats.damageTaken.ToString();
        }
    }
}
