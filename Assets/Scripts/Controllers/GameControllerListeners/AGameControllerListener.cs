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
    public abstract void OnEndMap(List<Unit> units, bool playerWon);
}
