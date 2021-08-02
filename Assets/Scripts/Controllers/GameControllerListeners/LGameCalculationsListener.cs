using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LGameCalculationsListener : AGameControllerListener
{
    public override void OnBeginPlayerTurn(List<Unit> units)
    {
        GameCalculations.EndTurnEvents(units);
    }

    public override void OnEndMap(List<Unit> units, bool playerWon)
    {
        GameCalculations.EndMapEvents(units);
    }
}
