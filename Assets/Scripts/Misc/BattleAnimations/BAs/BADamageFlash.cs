using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BADamageFlash : BattleAnimation
{
    public float BattleFlashTime;

    private void Update()
    {
        BattleFlashTime -= Time.deltaTime;
        if (BattleFlashTime <= 0)
        {
            ThisCombatant.Palette.Palette = (int)ThisCombatant.Unit.TheTeam;
        }
    }

    public override void Init(BattleAnimationController.CombatantData thisCombatant, BattleAnimationController.CombatantData otherCombatant, BattleAnimationController battleAnimationController)
    {
        base.Init(thisCombatant, otherCombatant, battleAnimationController);
        if (!ThisCombatant.Unit.Statue)
        {
            ThisCombatant.Palette.Palette = 3;
        }
        else
        {
            Destroy(this);
        }
    }
}
