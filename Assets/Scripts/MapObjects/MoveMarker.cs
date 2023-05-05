using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMarker : MarkerWithArrow
{
    public virtual Vector2Int TargetPos { get => Pos; set => Pos = value; }

    public override void Hover(InteractState interactState)
    {
        if (interactState == InteractState.Move && Origin.TheTeam == GameController.Current.CurrentPhase)
        {
            ShowArrowPath();
        }
    }

    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.Move && Origin.TheTeam == GameController.Current.CurrentPhase)
        {
            Origin.MoveOrder(TargetPos);
            SystemSFXController.Play(SystemSFXController.Type.UnitAction);
        }
        else if (interactState == InteractState.None)
        {
            GameController.Current.RemoveMarkers();
        }
    }
}
