using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TQuit : Trigger
{
    public override void Activate()
    {
        Application.Quit();
    }
}
