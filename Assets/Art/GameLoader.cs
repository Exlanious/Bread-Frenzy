using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;

    [Header("Input Settings")]
    [SerializeField] private bool useAnyKey = true;
    [SerializeField] private KeyCode specificKey = KeyCode.Return;
    [SerializeField] private float inputDelay = 0.5f;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup; // assign FadePanel here
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private bool fadeInOnStart = true;
    [SerializeField] private bool disableFadePanelAfterFadeIn = true;

    private bool inputEnabled = false;
    private bool isFading = false;

    private void Awake()
    {
        EnsureFadePanelIsUsable();
    }

    private void Start()
    {
        if (fadeCanvasGroup != null && fadeInOnStart)
        {
            // Start fully black and fade in
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(Fade(1f, 0f, fadeDuration, () =>
            {
                if (disableFadePanelAfterFadeIn)
                {
                    fadeCanvasGroup.gameObject.SetActive(false);
                }

                // After fade-in completes, enable input after delay
                StartCoroutine(EnableInputAfterDelay());
            }));
        }
        else
        {
            // No fade-in; just enable input after delay
            StartCoroutine(EnableInputAfterDelay());
        }
    }

    private void Update()
    {
        if (!inputEnabled || isFading) return;

        if (useAnyKey)
        {
            if (Input.anyKeyDown)
            {
                StartFadeAndLoad();
            }
        }
        else
        {
            if (Input.GetKeyDown(specificKey))
            {
                StartFadeAndLoad();
            }
        }
    }

    private void StartFadeAndLoad()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("GameLoader: No scene assigned to load.");
            return;
        }

        if (fadeCanvasGroup != null && !isFading)
        {
            isFading = true;
            // Make sure it's active in case it was disabled after fade-in
            EnsureFadePanelIsUsable();

            // Fade to black, then load next scene
            StartCoroutine(Fade(0f, 1f, fadeDuration, () =>
            {
                SceneManager.LoadScene(sceneToLoad);
            }));
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private IEnumerator EnableInputAfterDelay()
    {
        yield return new WaitForSeconds(inputDelay);
        inputEnabled = true;
    }

    private IEnumerator Fade(float from, float to, float duration, System.Action onComplete = null)
    {
        if (fadeCanvasGroup == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        EnsureFadePanelIsUsable();

        float elapsed = 0f;
        fadeCanvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        fadeCanvasGroup.alpha = to;
        onComplete?.Invoke();
    }

    private void EnsureFadePanelIsUsable()
    {
        if (fadeCanvasGroup == null) return;

        // Make sure the GameObject is active
        if (!fadeCanvasGroup.gameObject.activeSelf)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
        }

        // Make sure the component itself is enabled
        if (!fadeCanvasGroup.enabled)
        {
            fadeCanvasGroup.enabled = true;
        }
    }
}
