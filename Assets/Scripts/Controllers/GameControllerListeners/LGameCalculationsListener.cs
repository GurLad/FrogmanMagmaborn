using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LGameCalculationsListener : AGameControllerListener
{
    public override void OnBeginPlayerTurn(List<Unit> units)
    {
        GameCalculations.EndTurnEvents(units);
    }

    public override void OnEndLevel(List<Unit> units, bool playerWon)
    {
        GameCalculations.EndLevelEvents(units);
    }

    public override void OnPlayerWin(List<Unit> units)
    {
        GameCalculations.PlayerWinEvents(units);
    }
}
