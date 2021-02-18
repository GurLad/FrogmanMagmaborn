using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TQuit : Trigger
{
    public override void Activate()
    {
        SavedData.SaveAll();
        Application.Quit();
    }
}
