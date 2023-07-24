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
    [Header("Teleport animation")]
    public AdvancedSpriteSheetAnimation TeleportAnimation;
    public AudioClip TeleportSFX;
    [Header("SFX")]
    public AudioClip HitSFX;
    public AudioClip MissSFX;
    public AudioClip NoDamageSFX;
    [HideInInspector]
    public System.Action OnFinishAnimation;
    private Queue<MapAnimation> animations = new Queue<MapAnimation>();
    private MapAnimation currentAnimation;

    private void Awake()
    {
        Current = this;
    }

    public bool TryPlayNextAnimation()
    {
        if (animations.Count > 0 && !MidBattleScreen.HasCurrent && (currentAnimation == null || currentAnimation.Done))
        {
            (currentAnimation = animations.Dequeue()).StartAnimation();
            return true;
        }
        if (animations.Count > 0 && (currentAnimation == null || currentAnimation.Done))
        {
            return false;
        }
        else // No animation/currently running one is fine as well
        {
            return true;
        }
    }

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

    public void AnimateTeleport(Unit unit, Vector2Int targetPos, bool move)
    {
        CreateAnimation<MATeleport>((anim) => anim.Init(OnFinishAnimation, TeleportAnimation, TeleportSFX, unit, targetPos, move));
    }

    public void AnimateSwapTeleport(Unit unit1, Unit unit2)
    {
        CreateAnimation<MAMultiTeleport>((anim) => anim.Init(OnFinishAnimation, TeleportAnimation, TeleportSFX,
            new List<Unit> { unit1, unit2 }, new List<Vector2Int> { unit2.Pos, unit1.Pos }, true));
    }

    private bool CreateAnimation<T>(System.Func<T, bool> initFunc) where T : MapAnimation
    {
        T newAnim = gameObject.AddComponent<T>();
        if (initFunc(newAnim))
        {
            animations.Enqueue(newAnim);
            return TryPlayNextAnimation();
        }
        return false;
    }
}
