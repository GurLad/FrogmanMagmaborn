using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMarker : Marker
{
    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.Move && Origin.TheTeam == GameController.Current.CurrentPhase)
        {
            Origin.MoveOrder(Pos);
        }
        else if (interactState == InteractState.None)
        {
            GameController.Current.RemoveMarkers();
        }
    }
}
