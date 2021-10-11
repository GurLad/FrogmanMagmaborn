using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BATeleportBack : BattleAnimation
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
                temp.x = ThisCombatant.InitPos;
                ThisCombatant.Object.transform.position = temp;
                break;
            case "TeleportEnd":
                ThisCombatant.Animation.Activate("Idle");
                Finish();
                break;
            default:
                break;
        }
    }

    public override void Init(BattleAnimationController.CombatantData thisCombatant, BattleAnimationController.CombatantData otherCombatant, BattleAnimationController battleAnimationController)
    {
        base.Init(thisCombatant, otherCombatant, battleAnimationController);
        if (ThisCombatant.Unit.CanAttack(OtherCombatant.Unit)) // If can't, they never teleported to the target, so they don't need to come back
        {
            Debug.Log("Tele");
            ThisCombatant.Animation.Activate("TeleportStart");
        }
        else
        {
            Finish();
        }
    }
}
