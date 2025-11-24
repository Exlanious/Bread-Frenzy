using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private bool disableFadePanelAfterFadeIn = true;

    private void Awake()
    {
        EnsureFadePanelIsUsable();

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
        }
    }

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;

        if (disableFadePanelAfterFadeIn && fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void EnsureFadePanelIsUsable()
    {
        if (fadeCanvasGroup == null) return;

        if (!fadeCanvasGroup.gameObject.activeSelf)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
        }

        if (!fadeCanvasGroup.enabled)
        {
            fadeCanvasGroup.enabled = true;
        }
    }
}
