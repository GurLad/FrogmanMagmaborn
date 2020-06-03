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
            Unit unit = GameController.Current.FindUnitAtPos(Pos.x, Pos.y);
            if (unit != null)
            {
                if (unit.TheTeam != Origin.TheTeam)
                {
                    // Fight!
                    Origin.Attack(unit);
                    // ...and counter
                    unit.Attack(Origin);
                    GameController.Current.FinishMove(Origin);
                }
                else if (unit == Origin)
                {
                    GameController.Current.FinishMove(Origin);
                }
            }
        }
    }
}
