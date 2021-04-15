using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TShowMenu : Trigger
{
    public MenuController This;
    public MenuController Target;

    public override void Activate()
    {
        if (This != null)
        {
            This.Finish();
        }
        Target.Begin();
    }
}
