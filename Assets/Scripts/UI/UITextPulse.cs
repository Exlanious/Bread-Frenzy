using UnityEngine;
using TMPro;

public class UITextPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseScale = 1.05f;
    public float pulseSpeed = 1.5f;

    [Header("Fade Settings")]
    [Range(0f, 1f)]
    public float minAlpha = 0.4f;

    private TMP_Text text;
    private Vector3 originalScale;
    private Color originalColor;
    private float timer;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        originalScale = transform.localScale;
        originalColor = text.color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float scale = Mathf.Lerp(1f, pulseScale, (Mathf.Sin(timer * pulseSpeed) + 1f) / 2f);
        transform.localScale = originalScale * scale;

        float alpha = Mathf.Lerp(minAlpha, 1f, (Mathf.Sin(timer * pulseSpeed) + 1f) / 2f);
        Color c = originalColor;
        c.a = alpha;
        text.color = c;
    }
}
