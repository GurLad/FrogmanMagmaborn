using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BACounterAttack : BattleAnimation
{
    public override void FinishedAnimation(int id, string name)
    {
        base.FinishedAnimation(id, name);
        Finish();
    }

    public override void Init(BattleAnimationController.CombatantData thisCombatant, BattleAnimationController.CombatantData otherCombatant, BattleAnimationController battleAnimationController)
    {
        base.Init(thisCombatant, otherCombatant, battleAnimationController);
        if (!ThisCombatant.Unit.CanAttack(OtherCombatant.Unit))
        {
            Finish();
            return;
        }
        float temp = OtherCombatant.Object.transform.position.z;
        OtherCombatant.Object.transform.position += new Vector3(0, 0, ThisCombatant.Object.transform.position.z - temp);
        ThisCombatant.Object.transform.position -= new Vector3(0, 0, ThisCombatant.Object.transform.position.z - temp);
        ThisCombatant.Object.transform.position = new Vector3(OtherCombatant.Object.transform.position.x + ThisCombatant.LookingLeftSign, ThisCombatant.Object.transform.position.y, ThisCombatant.Object.transform.position.z);
        if (ThisCombatant.Animation.HasAnimation("CounterStart"))
        {
            ThisCombatant.Animation.Activate("CounterStart");
        }
        else
        {
            Finish();
        }
    }
}
