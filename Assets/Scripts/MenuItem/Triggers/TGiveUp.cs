using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGiveUp : Trigger
{
    public override void Activate()
    {
        GameController.Current.Lose();
    }
}
