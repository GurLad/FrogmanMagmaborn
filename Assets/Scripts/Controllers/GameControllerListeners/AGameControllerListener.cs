using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AGameControllerListener : MonoBehaviour, IGameControllerListener
{
    protected virtual void Start()
    {
        GameController.Current.AddListener(this);
    }

    protected virtual void OnDestroy()
    {
        GameController.Current.RemoveListener(this);
    }

    public abstract void OnBeginPlayerTurn(List<Unit> units);
    public abstract void OnEndLevel(List<Unit> units, bool playerWon);
    public abstract void OnPlayerWin(List<Unit> units);
    public virtual void OnUnitDeath(string unitName) { } // Since most listeners don't need it
}
