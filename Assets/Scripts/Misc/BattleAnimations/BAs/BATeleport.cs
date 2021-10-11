using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BATeleport : BattleAnimation
{
    public override void FinishedAnimation(int id, string name)
    {
        base.FinishedAnimation(id, name);
        switch (name)
        {
            case "TeleportStart":
                ThisCombatant.Animation.Activate("TeleportEnd");
                ThisCombatant.LookingLeft = !ThisCombatant.LookingLeft;
                Vector3 temp = ThisCombatant.Object.transform.position;
                temp.x = OtherCombatant.Object.transform.position.x + OtherCombatant.LookingLeftSign;
                ThisCombatant.Object.transform.position = temp;
                break;
            case "TeleportEnd":
                Finish();
                break;
            default:
                break;
        }
    }

    public override void Init(BattleAnimationController.CombatantData thisCombatant, BattleAnimationController.CombatantData otherCombatant, BattleAnimationController battleAnimationController)
    {
        base.Init(thisCombatant, otherCombatant, battleAnimationController);
        if (ThisCombatant.Unit.CanAttack(OtherCombatant.Unit))
        {
            // Fix looking left for backstabs (teleport mirror match)
            ThisCombatant.LookingLeft = !OtherCombatant.LookingLeft;
            ThisCombatant.Animation.Activate("TeleportStart");
        }
        else
        {
            Finish();
        }
    }
}
