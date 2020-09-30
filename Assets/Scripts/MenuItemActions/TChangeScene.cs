using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TChangeScene : Trigger
{
    public string SceneName;
    public override void Activate()
    {
        SceneManager.LoadScene(SceneName);
    }
}
