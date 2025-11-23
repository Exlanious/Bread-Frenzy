using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public string sceneToLoad;
    public bool onclick = false;
    public KeyCode keyToLoad = KeyCode.Mouse0;
    [SerializeField] private bool active = false;

    void Start()
    {
        StartCoroutine(wait());
    }

    public void LoadScene()
    {
        Debug.Log("Loading Scene: " + sceneToLoad);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }

    void Update()
    {
        if (onclick && active && Input.GetKeyDown(keyToLoad))
        {
            LoadScene();
        }
    }
    IEnumerator wait()
    {
        yield return new WaitForSeconds(0.5f);
        active = true;
    }
}
