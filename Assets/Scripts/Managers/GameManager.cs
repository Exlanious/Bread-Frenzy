using UnityEngine;

public class GameManager : MonoBehaviour
{
    //link to other game wide managers here.
    public CameraFollow cameraFollow;
    public Experience experience;

    public static GameManager Instance; //singleton

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void CursorLock(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void PauseGame(bool isPaused)
    {
        if (isPaused)
        {
            PauseManager.SetPaused(PauseSource.Menu, true);
        }
        else
        {
            PauseManager.SetPaused(PauseSource.Menu, false);
        }
    }
}
