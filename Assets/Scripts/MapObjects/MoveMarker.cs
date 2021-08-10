using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMarker : Marker
{
    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.Move && Origin.TheTeam == Team.Player)
        {
            GameController.Current.RemoveMarkers();
            MapAnimationsController.Current.OnFinishAnimation = () =>
            {
                Origin.MarkAttack();
                GameController.Current.ShowPointerMarker(Origin, 3);
                GameController.Current.InteractState = InteractState.Attack;
            };
            Origin.MoveTo(Pos);
        }
        else if (interactState == InteractState.None)
        {
            GameController.Current.RemoveMarkers();
        }
    }
}
