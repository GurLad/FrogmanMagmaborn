using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstSceneLoader : MonoBehaviour
{
    private enum State { FirstFrame, ActivateImporter, Wait, LoadScene }
    public FrogForgeImporter FrogForgeImporter;
    public DebugOptions DebugOptions;
    public GameObject FrogForgeLogo;
    public GameObject LoadingTextObject;
    private State state;

    private void Awake()
    {
#if MODDABLE_BUILD && !UNITY_EDITOR
        FrogForgeLogo.SetActive(true);
#else
        LoadingTextObject.SetActive(true);
#endif
    }

    private void Update()
    {
        switch (state)
        {
            case State.FirstFrame:
                state = State.ActivateImporter;
                break;
            case State.ActivateImporter:
#if MODDABLE_BUILD && !UNITY_EDITOR
                FrogForgeImporter.gameObject.SetActive(true);
                state = State.Wait;
#else
                state = State.LoadScene;
#endif
                break;
            case State.Wait:
                if (Time.timeSinceLevelLoad >= 1)
                {
                    state = State.LoadScene;
                }
                break;
            case State.LoadScene:
                string target = DebugOptions.SkipIntro ? "Map" : "Menu";
                SceneManager.LoadSceneAsync(target);
                Destroy(this);
                break;
            default:
                break;
        }
    }
}
