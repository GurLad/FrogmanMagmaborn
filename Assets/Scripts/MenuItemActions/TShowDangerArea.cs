using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TShowDangerArea : Trigger
{
    public override void Activate()
    {
        GameController.Current.ShowDangerArea();
    }
}
