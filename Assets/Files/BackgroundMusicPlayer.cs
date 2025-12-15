using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicAndAmbience : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip musicClip;
    public AudioClip ambienceClip;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;
    public bool loopMusic = true;
    public bool loopAmbience = true;

    private AudioSource musicSource;
    private AudioSource ambienceSource;

    private void Awake()
    {
        // Don't destroy on load
        DontDestroyOnLoad(gameObject);

        // Create two audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = loopMusic;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.clip = ambienceClip;
        ambienceSource.loop = loopAmbience;
        ambienceSource.volume = ambienceVolume;
        ambienceSource.playOnAwake = false;

        // Play both tracks
        musicSource.Play();
        ambienceSource.Play();
    }

    private void Update()
    {
        // Pause both if timescale is 0
        if (Time.timeScale == 0f)
        {
            if (musicSource.isPlaying) musicSource.Pause();
            if (ambienceSource.isPlaying) ambienceSource.Pause();
        }
        else
        {
            if (!musicSource.isPlaying) musicSource.UnPause();
            if (!ambienceSource.isPlaying) ambienceSource.UnPause();
        }
    }

    public void StopAllAudio()
    {
        musicSource.Stop();
        ambienceSource.Stop();
    }

    public void FadeMusic(float targetVolume, float duration)
    {
        StartCoroutine(FadeCoroutine(musicSource, targetVolume, duration));
    }

    public void FadeAmbience(float targetVolume, float duration)
    {
        StartCoroutine(FadeCoroutine(ambienceSource, targetVolume, duration));
    }

    private System.Collections.IEnumerator FadeCoroutine(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }
}
