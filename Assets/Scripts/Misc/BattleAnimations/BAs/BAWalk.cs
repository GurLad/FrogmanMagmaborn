using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAWalk : BattleAnimation
{
    private const float SPEED = 2;
    private Vector3 currentPos;
    private float targetPos = 3;

    private void Update()
    {
        currentPos.x -= Time.deltaTime * SPEED * ThisCombatant.LookingLeftSign;
        if (currentPos.x * ThisCombatant.LookingLeftSign <= targetPos * ThisCombatant.LookingLeftSign)
        {
            currentPos.x = targetPos;
            Finish();
        }
        ThisCombatant.Object.transform.position = currentPos;
    }

    public override void Init(BattleAnimationController.CombatantData thisCombatant, BattleAnimationController.CombatantData otherCombatant, BattleAnimationController battleAnimationController)
    {
        base.Init(thisCombatant, otherCombatant, battleAnimationController);
        if (ThisCombatant.Unit.CanAttack(OtherCombatant.Unit))
        {
            // Fix looking left for backstabs (teleport)
            ThisCombatant.MoveInFront(OtherCombatant);
            ThisCombatant.LookingLeft = !OtherCombatant.LookingLeft;
            ThisCombatant.Animation.Activate("Walk");
            currentPos = ThisCombatant.Object.transform.position;
            targetPos = OtherCombatant.Object.transform.position.x + ThisCombatant.LookingLeftSign;
        }
        else
        {
            Finish();
        }
    }
}
