using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMarker : Marker
{
    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.Attack && Origin.TheTeam == Team.Player)
        {
            Unit unit = GameController.Current.FindUnitAtPos(Pos.x, Pos.y);
            if (unit != null)
            {
                if (unit.TheTeam.IsEnemy(Origin.TheTeam))
                {
                    // Fight!
                    Origin.Fight(unit);
                }
                else if (unit == Origin)
                {
                    GameController.Current.FinishMove(Origin);
                }
            }
        }
        else if (interactState == InteractState.None)
        {
            GameController.Current.RemoveMarkers();
        }
    }
}
