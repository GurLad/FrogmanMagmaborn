using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAnimationsController : MidBattleScreen
{
    public static MapAnimationsController Current;
    [Header("Movement animation")]
    public float WalkSpeed;
    public AudioClip WalkSound;
    [Header("Delay animation")]
    public float DelayTime;
    [Header("Battle animation")]
    public float BattleSpeed;
    public float BattleFlashTime;
    public float BattleMoveDistance;
    public AdvancedSpriteSheetAnimation BattleMissAnimation;
    public RectTransform BattleBasePanelPosition;
    [Header("Push & pull animations")]
    public float PushPullApproachDistance;
    public float PushPullApproachRetreatSpeed;
    public float PushPullMoveSpeed;
    [Header("SFX")]
    public AudioClip HitSFX;
    public AudioClip MissSFX;
    public AudioClip NoDamageSFX;
    [HideInInspector]
    public System.Action OnFinishAnimation;

    public void AnimateMovement(Unit unit, Vector2Int targetPos)
    {
        CreateAnimation<MAMovement>((anim) => anim.Init(OnFinishAnimation, WalkSpeed, WalkSound, unit, targetPos));
    }

    public void AnimateBattle(Unit attacking, Unit defending, float attackerRandomResult, float defenderRandomResult)
    {
        CreateAnimation<MABattle>((anim) =>
            anim.Init(OnFinishAnimation,
                BattleSpeed,
                BattleFlashTime,
                BattleMoveDistance,
                BattleMissAnimation,
                BattleBasePanelPosition,
                HitSFX, MissSFX, NoDamageSFX,
                attacking, defending, attackerRandomResult, defenderRandomResult));
    }

    public void AnimatePushPull(Unit attacking, Unit defending, bool push)
    {
        CreateAnimation<MAPushPull>((anim) => anim.Init(OnFinishAnimation, PushPullApproachDistance, PushPullApproachRetreatSpeed, PushPullMoveSpeed, HitSFX, attacking, defending, push));
    }

    public void AnimateDelay()
    {
        CreateAnimation<MADelay>((anim) => anim.Init(OnFinishAnimation, DelayTime));
    }

    private bool CreateAnimation<T>(System.Func<T, bool> initFunc) where T : MapAnimation
    {
        T newAnim = gameObject.AddComponent<T>();
        if (initFunc(newAnim))
        {
            newAnim.StartAnimation();
            return true;
        }
        return false;
    }
}
