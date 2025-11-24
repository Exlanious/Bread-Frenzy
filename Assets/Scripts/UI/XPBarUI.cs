using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;        // circular image
    public TextMeshProUGUI levelText;

    // currentXP can be a float so we can animate smoothly
    public void SetXP(float currentXP, int xpToNextLevel, int level)
    {
        float fill = 0f;

        if (xpToNextLevel > 0)
            fill = currentXP / xpToNextLevel;

        fill = Mathf.Clamp01(fill);

        if (fillImage != null)
            fillImage.fillAmount = fill;

        if (levelText != null)
            levelText.text = level.ToString();
    }
}
