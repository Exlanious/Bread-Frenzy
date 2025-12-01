using System.Collections.Generic;
using UnityEngine;

public enum PauseSource
{
    GameOver,
    LevelUp,
    Menu,
    Hitstop,     
}

public static class PauseManager
{
    private static readonly HashSet<PauseSource> activeSources = new HashSet<PauseSource>();

    public static bool IsPaused => activeSources.Count > 0;

    public static void SetPaused(PauseSource source, bool paused)
    {
        bool changed = false;

        if (paused)
        {
            if (activeSources.Add(source))
                changed = true;
        }
        else
        {
            if (activeSources.Remove(source))
                changed = true;
        }

        if (changed)
            UpdateTimeScale();
            Debug.Log($"[PauseManager] source={source}, paused={paused}, active=[{string.Join(", ", activeSources)}], timeScale={Time.timeScale}");
    }

    private static void UpdateTimeScale()
    {
        Time.timeScale = activeSources.Count > 0 ? 0f : 1f;
    }

    public static void ForceClearAll()
    {
        activeSources.Clear();
        UpdateTimeScale();
    }
}
