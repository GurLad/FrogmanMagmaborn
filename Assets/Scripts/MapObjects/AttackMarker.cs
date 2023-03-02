using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMarker : MarkerWithArrow
{
    [HideInInspector]
    public Unit.DangerArea DangerArea;

    public override void Hover(InteractState interactState)
    {
        if (interactState == InteractState.Move && Origin.TheTeam == GameController.Current.CurrentPhase)
        {
            GameController.Current.GetMarkerAtPos<MarkerWithArrow>(DangerArea.GetBestPosToAttackTargetFrom(Pos)).ShowArrowPath();
        }
    }

    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.Attack && Origin.TheTeam == GameController.Current.CurrentPhase)
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
                else if (Origin.CanPush(unit))
                {
                    Origin.Push(unit);
                }
                else if (Origin.CanPull(unit))
                {
                    Origin.Pull(unit);
                }
                SystemSFXController.Play(SystemSFXController.Type.UnitAction);
            }
        }
        else if (interactState == InteractState.Move && Origin.TheTeam == GameController.Current.CurrentPhase)
        {
            Origin.MoveOrder(DangerArea.GetBestPosToAttackTargetFrom(Pos));
            SystemSFXController.Play(SystemSFXController.Type.UnitAction);
        }
        else if (interactState == InteractState.None)
        {
            GameController.Current.RemoveMarkers();
        }
    }
}
