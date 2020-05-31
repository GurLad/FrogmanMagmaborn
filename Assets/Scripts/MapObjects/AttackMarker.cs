using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMarker : Marker
{
    public Unit Origin;
    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.Attack && Origin.TheTeam == Team.Player)
        {
            Unit unit = GameController.Current.FindUnitAtPos(Origin.Pos.x, Origin.Pos.y);
            if (unit != null)
            {
                // Fight!
            }
        }
    }
}
