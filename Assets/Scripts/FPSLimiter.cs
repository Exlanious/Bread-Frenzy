using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [Header("Target FPS")]
    public int targetFPS = 60;

    void Awake()
    {
        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount = 1; 

        DontDestroyOnLoad(gameObject); 
    }
}
