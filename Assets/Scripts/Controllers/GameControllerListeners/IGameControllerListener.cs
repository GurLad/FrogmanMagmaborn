using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameControllerListener
{
    public void OnBeginPlayerTurn(List<Unit> units);
    public void OnEndLevel(List<Unit> units, bool playerWon);
    public void OnPlayerWin(List<Unit> units);
    public void OnUnitDeath(string unitName);
}
