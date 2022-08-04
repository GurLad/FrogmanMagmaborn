using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BAWalk : BattleAnimation
{
    private Vector3 currentPos;
    private float targetPos = 3;
    private bool walking;
    private float speed = 2;

    private void Update()
    {
        if (walking)
        {
            currentPos.x -= Time.deltaTime * speed * ThisCombatant.LookingLeftSign;
            if (currentPos.x * ThisCombatant.LookingLeftSign <= targetPos * ThisCombatant.LookingLeftSign)
            {
                currentPos.x = targetPos;
                Finish();
            }
            ThisCombatant.Object.transform.position = currentPos;
        }
    }

    public override void FinishedAnimation(int id, string name)
    {
        base.FinishedAnimation(id, name);
        if (name == "CounterStart")
        {
            ThisCombatant.Animation.Activate("Walk");
            walking = true;
        }
    }

    public override void Init(BattleAnimationController.CombatantData thisCombatant, BattleAnimationController.CombatantData otherCombatant, BattleAnimationController battleAnimationController)
    {
        base.Init(thisCombatant, otherCombatant, battleAnimationController);
        if (ThisCombatant.Unit.CanAttack(OtherCombatant.Unit))
        {
            // Fix looking left for backstabs (teleport)
            ThisCombatant.MoveInFront(OtherCombatant);
            ThisCombatant.LookingLeft = !OtherCombatant.LookingLeft;
            currentPos = ThisCombatant.Object.transform.position;
            targetPos = OtherCombatant.Object.transform.position.x + ThisCombatant.LookingLeftSign;
            speed = ThisCombatant.ClassAnimationData.WalkExtraData.CustomSpeed ? ThisCombatant.ClassAnimationData.WalkExtraData.Speed : speed;
            if (ThisCombatant.Animation.HasAnimation("CounterStart"))
            {
                ThisCombatant.Animation.Activate("CounterStart");
            }
            else
            {
                ThisCombatant.Animation.Activate("Walk");
                walking = true;
            }
        }
        else
        {
            Finish();
        }
    }
}
