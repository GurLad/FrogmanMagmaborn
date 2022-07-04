using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAMeleeAttack : BattleAnimation
{
    public override void FinishedAnimation(int id, string name)
    {
        base.FinishedAnimation(id, name);
        switch (name)
        {
            case "AttackStart":
                ThisCombatant.Animation.Activate("AttackEnd");
                // Fix looking left for backstabs (teleport)
                OtherCombatant.LookingLeft = !ThisCombatant.LookingLeft;
                BattleAnimationController.HandleDamage(ThisCombatant, OtherCombatant);
                break;
            case "AttackEnd":
                if (ThisCombatant.Animation.HasAnimation("IdlePost"))
                {
                    ThisCombatant.Animation.Activate("IdlePost");
                }
                else
                {
                    ThisCombatant.Animation.Activate("Idle");
                }
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
            if (ThisCombatant.Animation.HasAnimation("AttackStart"))
            {
                ThisCombatant.Animation.Activate("AttackStart");
            }
            else
            {
                ThisCombatant.Animation.Activate("AttackEnd");
                // Fix looking left for backstabs (teleport)
                OtherCombatant.LookingLeft = !ThisCombatant.LookingLeft;
                BattleAnimationController.HandleDamage(ThisCombatant, OtherCombatant);
            }
        }
        else
        {
            Finish();
        }
    }
}
