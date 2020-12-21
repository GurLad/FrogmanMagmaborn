using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEndTurn : Trigger
{
    public override void Activate()
    {
        GameController.Current.ManuallyEndTurn();
    }
}
