using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TShowMenu : Trigger
{
    public MenuController Target;

    public override void Activate()
    {
        MidBattleScreen.Set(Target, true);
        Target.gameObject.SetActive(true);
    }
}
