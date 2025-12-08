using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelector : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform cardCanvas;
    [SerializeField] private Button SkipButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup uiCanvasGroup;

    [Header("Card Styling")]
    [SerializeField] private TMP_FontAsset upgradeFont;

    [Header("Settings")]
    [SerializeField] private int numGeneratedCards = 3;
    [SerializeField] private float fadeDuration = 0.18f;
    [SerializeField] private float clickDelayAfterFade = 0.05f;

    [Header("Ability System")]
    [SerializeField] private AbilityUpgradeSelector abilitySelector;
    [SerializeField] private AbilityManager abilityManager;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cardSelectSound;



    private AudioSource sfxSource;


    private Queue<int> upgradeQueue = new Queue<int>();
    private bool isShowingUpgrade = false;

    private AbilityUpgrade[] currentOptions;
    private Coroutine fadeRoutine;

    void Start()
    {
        if (abilitySelector == null)
            abilitySelector = FindObjectOfType<AbilityUpgradeSelector>();

        if (abilityManager == null)
            abilityManager = FindObjectOfType<AbilityManager>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (uiCanvasGroup == null)
            uiCanvasGroup = GetComponent<CanvasGroup>();

        if (SkipButton != null)
        {
            SkipButton.onClick.RemoveAllListeners();
            SkipButton.onClick.AddListener(OnSkipPressed);
        }

        HideUI();
    }

    public void QueueUpgrade()
    {
        upgradeQueue.Enqueue(1);
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

        currentOptions = abilitySelector.GetRandomOptions(numGeneratedCards);
        if (currentOptions == null || currentOptions.Length == 0)
        {
            Debug.LogWarning("UpgradeSelector: No upgrade options available.");
            upgradeQueue.Dequeue();
            isShowingUpgrade = false;
            return;
        }

        if (backgroundImage != null)
            backgroundImage.enabled = true;

        if (cardCanvas != null)
            cardCanvas.gameObject.SetActive(true);

        if (SkipButton != null)
            SkipButton.gameObject.SetActive(true);

        foreach (Transform child in cardCanvas)
            Destroy(child.gameObject);

        for (int i = 0; i < currentOptions.Length; i++)
        {
            CreateUpgradeCard(currentOptions[i], i);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PauseManager.SetPaused(PauseSource.LevelUp, true);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeInAndEnableRoutine());
    }

    private IEnumerator FadeInAndEnableRoutine()
    {
        if (uiCanvasGroup == null)
            yield break;

        uiCanvasGroup.alpha = 0f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            uiCanvasGroup.alpha = normalized;
            yield return null;
        }

        if (clickDelayAfterFade > 0f)
            yield return new WaitForSecondsRealtime(clickDelayAfterFade);

        uiCanvasGroup.alpha = 1f;
        uiCanvasGroup.interactable = true;
        uiCanvasGroup.blocksRaycasts = true;
    }

    private void CreateUpgradeCard(AbilityUpgrade upgrade, int index)
    {
        GameObject cardObj = new GameObject("UpgradeCard_" + upgrade.upgradeName);
        cardObj.transform.SetParent(cardCanvas, false);

        var image = cardObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.90f);

        var button = cardObj.AddComponent<Button>();

        var rt = cardObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(550f, 720f);

        var layout = cardObj.AddComponent<LayoutElement>();
        layout.preferredWidth = 550f;
        layout.preferredHeight = 720f;

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(cardObj.transform, false);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = upgrade.upgradeName + "\n\n<size=55%>" + upgrade.description + "</size>";
        text.fontSize = 54;
        text.alignment = TextAlignmentOptions.Center;

        if (upgradeFont != null)
            text.font = upgradeFont;

        var rtText = text.GetComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.offsetMin = new Vector2(40, 40);
        rtText.offsetMax = new Vector2(-40, -40);

        var hover = cardObj.AddComponent<UpgradeCardHover>();
        hover.Init(image, text);

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

            if (audioSource != null && cardSelectSound != null)
            {
                audioSource.PlayOneShot(cardSelectSound);
            }

        }

        upgradeQueue.Dequeue();
        CloseAndContinue();
    }

    public void OnSkipPressed()
    {
        Debug.Log("Upgrade skipped.");

        if (audioSource != null && cardSelectSound != null)
        {
            audioSource.PlayOneShot(cardSelectSound);
        }


        if (upgradeQueue.Count > 0)
            upgradeQueue.Dequeue();

        CloseAndContinue();
    }

    private void CloseAndContinue()
    {
        HideUI();
        isShowingUpgrade = false;
        currentOptions = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PauseManager.SetPaused(PauseSource.LevelUp, false);

        TryShowNextUpgrade();
    }

    private void HideUI()
    {
        if (backgroundImage != null)
            backgroundImage.enabled = false;

        if (cardCanvas != null)
            cardCanvas.gameObject.SetActive(false);

        if (SkipButton != null)
            SkipButton.gameObject.SetActive(false);

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
    }
}
