using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("References")]
    public Image progressBarFill;

    [Header("Colors")]
    public Color healthyColor = Color.green;          // > 50%
    public Color midColor = new Color(1f, 0.8f, 0f);  // 20–50% (kinda yellow)
    public Color lowColor = Color.red;                // ≤ 20%

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        progressBarFill.fillAmount = progress;

        if (progress > 0.5f)
        {
            progressBarFill.color = healthyColor;
        }
        else if (progress > 0.2f)
        {
            progressBarFill.color = midColor;
        }
        else
        {
            progressBarFill.color = lowColor;
        }
    }
}
