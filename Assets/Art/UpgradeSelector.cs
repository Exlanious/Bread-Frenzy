using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelector : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform cardCanvas;
    [SerializeField] private Button SkipButton;

    [Header("Settings")]
    [SerializeField] private int numGeneratedCards = 3;

    [Header("Ability System")]
    [SerializeField] private AbilityUpgradeSelector abilitySelector;
    [SerializeField] private AbilityManager abilityManager;

    private Queue<int> upgradeQueue = new Queue<int>();
    private bool isShowingUpgrade = false;

    // Current options being shown
    private AbilityUpgrade[] currentOptions;

    void Start()
    {
        // Auto-find backend ability systems if not set
        if (abilitySelector == null)
            abilitySelector = FindObjectOfType<AbilityUpgradeSelector>();

        if (abilityManager == null)
            abilityManager = FindObjectOfType<AbilityManager>();

        HideUI();
    }

    /// <summary>
    /// Called by PlayerXP when you level up.
    /// Adds a pending upgrade choice and shows UI if not already showing.
    /// </summary>
    public void QueueUpgrade()
    {
        upgradeQueue.Enqueue(1);   // just a token to say "we owe one upgrade choice"
        TryShowNextUpgrade();
    }

    private void TryShowNextUpgrade()
    {
        if (isShowingUpgrade) return;
        if (upgradeQueue.Count == 0) return;

        ShowUpgrade();
    }

    private void ShowUpgrade()
    {
        isShowingUpgrade = true;

        if (abilitySelector == null || abilityManager == null)
        {
            Debug.LogError("UpgradeSelector: Missing AbilitySelector or AbilityManager reference.");
            upgradeQueue.Dequeue();
            isShowingUpgrade = false;
            return;
        }

        // Get unique upgrade options from backend
        currentOptions = abilitySelector.GetRandomOptions(numGeneratedCards);
        if (currentOptions == null || currentOptions.Length == 0)
        {
            Debug.LogWarning("UpgradeSelector: No upgrade options available.");
            upgradeQueue.Dequeue();
            isShowingUpgrade = false;
            return;
        }

        // show card UI
        cardCanvas.gameObject.SetActive(true);
        SkipButton.gameObject.SetActive(true);

        // clear old cards
        foreach (Transform child in cardCanvas)
            Destroy(child.gameObject);

        // generate UI cards for each option
        for (int i = 0; i < currentOptions.Length; i++)
        {
            CreateUpgradeCard(currentOptions[i], i);
        }

        // TEMP: pause game & unlock cursor
        GameManager.Instance.cameraFollow.enabled = false;
        GameManager.Instance.CursorLock(false);
        GameManager.Instance.PauseGame(true);
    }

    private void CreateUpgradeCard(AbilityUpgrade upgrade, int index)
    {
        // Create a UI Button object
        GameObject cardObj = new GameObject("UpgradeCard_" + upgrade.upgradeName);
        cardObj.transform.SetParent(cardCanvas, false);

        // Add Button + Image components
        var image = cardObj.AddComponent<Image>();
        var button = cardObj.AddComponent<Button>();

        // Simple placeholder style
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);

        // Size
        var rt = cardObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 140);

        // Add text child
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(cardObj.transform, false);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = upgrade.upgradeName + "\n<size=60%>" + upgrade.description + "</size>";
        text.fontSize = 30;
        text.alignment = TextAlignmentOptions.Center;

        var rtText = text.GetComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.offsetMin = Vector2.zero;
        rtText.offsetMax = Vector2.zero;

        // Capture index for the button callback
        int capturedIndex = index;
        button.onClick.AddListener(() => OnUpgradeSelected(capturedIndex));
    }

    private void OnUpgradeSelected(int optionIndex)
    {
        if (currentOptions != null && optionIndex >= 0 && optionIndex < currentOptions.Length)
        {
            AbilityUpgrade chosen = currentOptions[optionIndex];
            Debug.Log("Selected upgrade: " + chosen.upgradeName);

            if (abilityManager != null)
            {
                abilityManager.ApplyUpgrade(chosen);
            }
        }

        upgradeQueue.Dequeue();
        CloseAndContinue();
    }

    public void OnSkipPressed()
    {
        // Skip without applying any upgrade
        Debug.Log("Upgrade skipped.");

        if (upgradeQueue.Count > 0)
            upgradeQueue.Dequeue();

        CloseAndContinue();
    }

    private void CloseAndContinue()
    {
        HideUI();
        isShowingUpgrade = false;
        currentOptions = null;

        // TEMP: resume game & relock cursor
        GameManager.Instance.cameraFollow.enabled = true;
        GameManager.Instance.CursorLock(true);
        GameManager.Instance.PauseGame(false);

        // If we queued multiple level-ups, show the next one
        TryShowNextUpgrade();
    }

    private void HideUI()
    {
        // Hide the background panel image
        var img = GetComponent<Image>();
        if (img != null)
            img.enabled = false;

        // Hide the cards + skip button
        if (cardCanvas != null)
            cardCanvas.gameObject.SetActive(false);

        if (SkipButton != null)
            SkipButton.gameObject.SetActive(false);
    }
}
