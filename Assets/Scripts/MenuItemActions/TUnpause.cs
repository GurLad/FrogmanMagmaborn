using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TUnpause : Trigger
{
    public override void Activate()
    {
        MidBattleScreen.Current.Quit();
    }
}
