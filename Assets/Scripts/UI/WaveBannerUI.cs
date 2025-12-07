using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaveBannerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private Image backgroundImage;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.18f;
    [SerializeField] private float holdDuration = 1.3f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private Coroutine bannerRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStarted += HandleWaveStarted;
        }
    }

    private void OnDisable()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStarted -= HandleWaveStarted;
        }
    }

    private string GetDisplayNameForWaveType(WaveType waveType)
    {
        switch (waveType)
        {
            case WaveType.FastDuck:
                return "FAST DUCKS";
            case WaveType.MiniBoss:
                return "MINI-BOSS";
            default:
                return waveType.ToString().ToUpperInvariant();
        }
    }

    private void HandleWaveStarted(Wave wave, int waveNumber)
    {
        if (canvasGroup == null || titleText == null)
            return;

        string waveTypeName = GetDisplayNameForWaveType(wave.waveType);
        titleText.text = $"WAVE {waveNumber} – {waveTypeName}";

        if (subtitleText != null)
        {
            subtitleText.text = GetSubtitleForWaveType(wave.waveType);
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = GetColorForWaveType(wave.waveType);
        }

        if (bannerRoutine != null)
            StopCoroutine(bannerRoutine);

        bannerRoutine = StartCoroutine(BannerRoutine());
    }

    private IEnumerator BannerRoutine()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / fadeInDuration);
            canvasGroup.alpha = normalized;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        float holdT = 0f;
        while (holdT < holdDuration)
        {
            holdT += Time.unscaledDeltaTime;
            yield return null;
        }

        float outT = 0f;
        while (outT < fadeOutDuration)
        {
            outT += Time.unscaledDeltaTime;
            float normalized = 1f - Mathf.Clamp01(outT / fadeOutDuration);
            canvasGroup.alpha = normalized;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        bannerRoutine = null;
    }

    private string GetSubtitleForWaveType(WaveType waveType)
    {
        switch (waveType)
        {
            case WaveType.Normal:
                return "Stay alive.";
            case WaveType.Break:
                return "Catch your breath.";
            case WaveType.Power:
                return "Overwhelm the weak.";
            case WaveType.MiniBoss:
                return "A stronger duck appears...";
            case WaveType.Boss:
                return "Boss wave!";
            case WaveType.FastDuck: 
                return "They're so fast...!";
            default:
                return "";
        }
    }

    private Color GetColorForWaveType(WaveType waveType)
    {
        switch (waveType)
        {
            case WaveType.Normal:
                return new Color(0.1f, 0.3f, 0.6f, 0.8f);   // blue-ish
            case WaveType.Break:
                return new Color(0.1f, 0.5f, 0.2f, 0.8f);   // green-ish
            case WaveType.Power:
                return new Color(0.7f, 0.5f, 0.1f, 0.8f);   // gold-ish
            case WaveType.MiniBoss:
                return new Color(0.4f, 0.1f, 0.5f, 0.8f);   // purple-ish
            case WaveType.Boss:
                return new Color(0.6f, 0.1f, 0.1f, 0.9f);   // red-ish
            case WaveType.FastDuck:                         
                return new Color(0.1f, 0.6f, 0.6f, 0.85f);  // teal / speedy vibe
            default:
                return new Color(0f, 0f, 0f, 0.8f);
        }
    }

}
