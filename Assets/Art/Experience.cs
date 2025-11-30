using System;
using TMPro;
using UnityEngine;


public class Experience : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private ProgressBar experienceBar;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private TextMeshProUGUI experienceLabel;

    [Header("Other Elements")]
    [SerializeField] private UpgradeSelector upgradeSelector;

    [Header("Level Up Settings File")]
    [SerializeField] private ExperienceSO experienceSettings;

    [Header("Runtime")]
    public int level;
    public int currentExperience;
    public int requiredLevelExperience;

    [Header("Testing")]
    [SerializeField] private bool testingMode = false;
    [SerializeField] private KeyCode testingKey = KeyCode.E;
    [SerializeField] private int incrementExperience = 2;

    public event Action OnLevelUp;

    private void Start()
    {
        level = 1;
        currentExperience = 0;
        requiredLevelExperience = experienceSettings.GetExperienceForLevel(level);

        if (upgradeSelector == null)
        {
            Debug.LogError("No Upgrade Selector Attached!");
            return;
        }
        OnLevelUp += upgradeSelector.QueueUpgrade;
    }

    public void Update()
    {
        updateDisplay();

        //for testing purposes
        if (testingMode)
        {
            if (Input.GetKeyDown(testingKey))
            {
                AddExperience(incrementExperience);
            }

        }
    }

    public int GetTotalExperience()
    {
        return currentExperience + experienceSettings.GetExperienceForLevel(level);
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;

        while (currentExperience >= requiredLevelExperience)
        {
            LevelUp();
        }

        updateDisplay();
    }

    public void LevelUp()
    {
        currentExperience = Mathf.Max(currentExperience - requiredLevelExperience, 0);
        level++;
        requiredLevelExperience = experienceSettings.GetExperienceForLevel(level);

        OnLevelUp?.Invoke();
    }

    public void updateDisplay()
    {
        experienceBar.SetProgress((float)currentExperience / requiredLevelExperience);
        levelLabel.SetText($"Level: {level}");
        experienceLabel.SetText($"Experience: {currentExperience,3}");
    }



}