using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TChangeScene : Trigger
{
    public string SceneName;
    public bool Immediate;
    public override void Activate()
    {
        SceneController.LoadScene(SceneName, Immediate);
    }
}
