using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBarUI : MonoBehaviour
{
    [Header("Bar References")]
    [SerializeField] private Image fillImage;          
    [SerializeField] private TextMeshProUGUI levelText; 

    [Header("Juice Settings")]
    [SerializeField] private float popScale = 1.1f;
    [SerializeField] private float popDuration = 0.15f;

    private RectTransform barRoot;
    private Vector3 originalScale;
    private float popTimer = 0f;
    private bool isPopping = false;

    private void Awake()
    {
        barRoot = GetComponent<RectTransform>();
        if (barRoot != null)
            originalScale = barRoot.localScale;
    }

    public void SetXP(float currentXP, float xpToNextLevel, int level)
    {
        if (xpToNextLevel <= 0) return;

        float t = Mathf.Clamp01(currentXP / xpToNextLevel);

        if (fillImage != null)
            fillImage.fillAmount = t;

        if (levelText != null)
            levelText.text = $"LV. {level}";

        if (!isPopping && t > 0.98f)
        {
            StartPop();
        }
    }

    private void Update()
    {
        if (isPopping)
        {
            popTimer += Time.unscaledDeltaTime;
            float normalized = popTimer / popDuration;

            if (normalized >= 1f)
            {
                isPopping = false;
                popTimer = 0f;
                if (barRoot != null)
                    barRoot.localScale = originalScale;
            }
            else
            {
                float scale = Mathf.Lerp(1f, popScale, 1f - Mathf.Cos(normalized * Mathf.PI));
                if (barRoot != null)
                    barRoot.localScale = originalScale * scale;
            }
        }
    }

    private void StartPop()
    {
        isPopping = true;
        popTimer = 0f;
    }
}
