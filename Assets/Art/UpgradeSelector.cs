using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelector : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform cardCanvas;
    [SerializeField] private Button SkipButton;
    [SerializeField] private Image backgroundImage;   // NEW

    [Header("Card Styling")]
    [SerializeField] private TMP_FontAsset upgradeFont;   // 🔹 NEW

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
        if (abilitySelector == null)
            abilitySelector = FindObjectOfType<AbilityUpgradeSelector>();

        if (abilityManager == null)
            abilityManager = FindObjectOfType<AbilityManager>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // 🔹 Make sure SkipButton actually calls OnSkipPressed
        if (SkipButton != null)
        {
            SkipButton.onClick.RemoveAllListeners();
            SkipButton.onClick.AddListener(OnSkipPressed);
        }

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

        // show background
        if (backgroundImage != null)
            backgroundImage.enabled = true;

        // show card UI
        if (cardCanvas != null)
            cardCanvas.gameObject.SetActive(true);

        if (SkipButton != null)
            SkipButton.gameObject.SetActive(true);


        // clear old cards
        foreach (Transform child in cardCanvas)
            Destroy(child.gameObject);

        // generate UI cards for each option
        for (int i = 0; i < currentOptions.Length; i++)
        {
            CreateUpgradeCard(currentOptions[i], i);
        }

        // 🔹 SHOW CURSOR + pause game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Simple pause without GameManager dependency
        Time.timeScale = 0f;
    }

    private void CreateUpgradeCard(AbilityUpgrade upgrade, int index)
    {
        // --- Create the card object ---
        GameObject cardObj = new GameObject("UpgradeCard_" + upgrade.upgradeName);
        cardObj.transform.SetParent(cardCanvas, false);

        // Background Image
        var image = cardObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.90f);

        // Button
        var button = cardObj.AddComponent<Button>();

        // BIG CARD SIZE (550 × 720)
        var rt = cardObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(550f, 720f);

        // Let HorizontalLayoutGroup respect this
        var layout = cardObj.AddComponent<LayoutElement>();
        layout.preferredWidth  = 550f;
        layout.preferredHeight = 720f;

        // --- TEXT ---
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(cardObj.transform, false);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = upgrade.upgradeName + "\n\n<size=55%>" + upgrade.description + "</size>";
        text.fontSize = 54;
        text.alignment = TextAlignmentOptions.Center;

        // Use custom font if assigned
        if (upgradeFont != null)
            text.font = upgradeFont;

        // Text RectTransform (THIS is the rtText you were missing!)
        var rtText = text.GetComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.offsetMin = new Vector2(40, 40);   // padding
        rtText.offsetMax = new Vector2(-40, -40);

        // --- Hover effect script ---
        var hover = cardObj.AddComponent<UpgradeCardHover>();
        hover.Init(image, text);

        // --- Click Handler ---
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

        // 🔹 HIDE CURSOR + unpause
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        // If we queued multiple level-ups, show the next one
        TryShowNextUpgrade();
    }

    private void HideUI()
    {
        // Hide the background panel image
        if (backgroundImage != null)
            backgroundImage.enabled = false;

        // Hide the cards + skip button
        if (cardCanvas != null)
            cardCanvas.gameObject.SetActive(false);

        if (SkipButton != null)
            SkipButton.gameObject.SetActive(false);
    }
}
