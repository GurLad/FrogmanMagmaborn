using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameControllerListener
{
    public void OnBeginPlayerTurn(List<Unit> units);
    public void OnEndMap(List<Unit> units, bool playerWon);
    // public void OnUnitDead(string unitName); // Maybe in the future
}
