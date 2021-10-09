using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleAnimation : MonoBehaviour, IAdvancedSpriteSheetAnimationListener
{
    protected BattleAnimationController BattleAnimationController;
    protected BattleAnimationController.CombatantData ThisCombatant;
    protected BattleAnimationController.CombatantData OtherCombatant;

    public virtual void ChangedFrame(int id, string name, int newFrame)
    {
        // Do nothing
    }

    public virtual void FinishedAnimation(int id, string name)
    {
        // Do nothing
    }

    public virtual void Init(BattleAnimationController.CombatantData thisCombatant, BattleAnimationController.CombatantData otherCombatant, BattleAnimationController battleAnimationController)
    {
        ThisCombatant = thisCombatant;
        OtherCombatant = otherCombatant;
        BattleAnimationController = battleAnimationController;
        ThisCombatant.Animation.Listeners.Add(this);
    }
}
