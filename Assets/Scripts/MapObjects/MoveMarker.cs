using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMarker : Marker
{
    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.Move && Origin.TheTeam == Team.Player)
        {
            Origin.MoveTo(Pos);
            GameController.Current.RemoveMarkers();
            Origin.MarkAttack();
            GameController.Current.InteractState = InteractState.Attack;
        }
    }
}
