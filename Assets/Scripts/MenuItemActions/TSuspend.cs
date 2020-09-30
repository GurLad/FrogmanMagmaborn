using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSuspend : Trigger
{
    public override void Activate()
    {
        // Save all level data (players, enemies, turn, map, event etc.). For save & quit/save & return to menu.
        // Currently does nothing, to make players accidently give up, like early Hades! Muhahaha!
    }
}
