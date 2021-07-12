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
                if (unit.TheTeam != Origin.TheTeam)
                {
                    // Fight!
                    GameController.Current.Target = unit; // Very bad workaround
                    if (ConversationPlayer.Current?.CheckWait() ?? false)
                    {
                        ConversationPlayer.Current.OnFinishConversation = () => Origin.Fight(unit);
                    }
                    else
                    {
                        Origin.Fight(unit);
                    }
                }
                else if (unit == Origin)
                {
                    GameController.Current.FinishMove(Origin);
                }
            }
        }
    }
}
