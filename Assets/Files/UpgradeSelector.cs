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

    private Queue<int> upgradeQueue = new Queue<int>();
    private bool isShowingUpgrade = false;

    void Start()
    {
        HideUI();
    }

    public void QueueUpgrade()
    {
        upgradeQueue.Enqueue(1);   // push a pending upgrade
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

        // show card UI
        cardCanvas.gameObject.SetActive(true);
        SkipButton.gameObject.SetActive(true);

        // clear old cards
        foreach (Transform child in cardCanvas)
            Destroy(child.gameObject);

        // generate new upgrade cards
        for (int i = 0; i < numGeneratedCards; i++)
        {
            CreateUpgradeCard();
        }
        //temp
        GameManager.Instance.cameraFollow.enabled = false;
        GameManager.Instance.CursorLock(false);
        GameManager.Instance.PauseGame(true);
    }

    // THIS IS TEMPORARY. 
    private void CreateUpgradeCard()
    {
        // Create a UI Button
        GameObject cardObj = new GameObject("TempUpgradeCard");
        cardObj.transform.SetParent(cardCanvas, false);

        // Add Button + Image components
        var image = cardObj.AddComponent<UnityEngine.UI.Image>();
        var button = cardObj.AddComponent<UnityEngine.UI.Button>();

        // Make it visible (simple placeholder color)
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.75f); // grey box

        // Set size (temporary)
        var rt = cardObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(250, 120);

        // Add text
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(cardObj.transform, false);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "TEMP UPGRADE";
        text.fontSize = 32;
        text.alignment = TMPro.TextAlignmentOptions.Center;

        // Stretch text to fit button
        var rtText = text.GetComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.offsetMin = Vector2.zero;
        rtText.offsetMax = Vector2.zero;

        // Add click callback
        button.onClick.AddListener(() => OnUpgradeSelected());
    }

    public void OnUpgradeSelected()
    {
        // Called by clicking a card
        upgradeQueue.Dequeue();
        HideUI();
        isShowingUpgrade = false;
        TryShowNextUpgrade();
        //temp
        GameManager.Instance.cameraFollow.enabled = true;
        GameManager.Instance.CursorLock(true);
        GameManager.Instance.PauseGame(false);
    }

    public void OnSkipPressed()
    {
        upgradeQueue.Dequeue();
        HideUI();
        isShowingUpgrade = false;
        TryShowNextUpgrade();
        //temp
        GameManager.Instance.cameraFollow.enabled = true;
        GameManager.Instance.CursorLock(true);
        GameManager.Instance.PauseGame(false);
    }

    private void HideUI()
    {
        cardCanvas.gameObject.SetActive(false);
        SkipButton.gameObject.SetActive(false);
    }
}
