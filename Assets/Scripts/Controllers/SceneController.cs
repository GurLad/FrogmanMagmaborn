using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private static string target;
    private bool frameDone;
    public static void LoadScene(string sceneName, bool immediate = false)
    {
        if (immediate)
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            target = sceneName;
            SceneManager.LoadScene("LoadingScreen");
        }
    }
    private void Update()
    {
        if (frameDone)
        {
            SceneManager.LoadSceneAsync(target);
            Destroy(this);
        }
        else
        {
            frameDone = true;
        }
    }
}
