using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEndTurn : Trigger
{
    public override void Activate()
    {
        GameController.Current.RemoveMarkers();
        GameController.Current.StartPhase((Team)((int)Team.Player + 1));
    }
}
