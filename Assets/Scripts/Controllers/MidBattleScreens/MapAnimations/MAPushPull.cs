using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MAPushPull : MapAnimation
{
    private enum PushPullAnimationState { Approach, Move, Retreat }

    public float PushPullApproachDistance;
    public float PushPullApproachRetreatSpeed;
    public float PushPullMoveSpeed;
    public AudioClip HitSFX;
    // Battle, push & pull animations vars
    private Unit attacker;
    private Unit defender;
    private Vector3 attackerBasePos;
    private Vector3 defenderBasePos;
    private Vector3 battleDirection;
    // Push & pull animations vars
    private PushPullAnimationState pushPullState;
    private Vector2Int pushPullPosModifier;
    private bool push;

    public bool Init(System.Action onFinishAnimation, 
        float pushPullApproachDistance, float pushPullApproachRetreatSpeed, float pushPullMoveSpeed, AudioClip hitSFX,
        Unit attacking, Unit defending, bool push)
    {
        // Constructor
        OnFinishAnimation = onFinishAnimation;
        PushPullApproachDistance = pushPullApproachDistance;
        PushPullApproachRetreatSpeed = pushPullApproachRetreatSpeed;
        PushPullMoveSpeed = pushPullMoveSpeed;
        HitSFX = hitSFX;
        // Init
        attacker = attacking;
        defender = defending;
        attackerBasePos = attacker.transform.position;
        defenderBasePos = defender.transform.position;
        battleDirection = (defenderBasePos - attackerBasePos).normalized;
        pushPullState = PushPullAnimationState.Approach;
        pushPullPosModifier = (attacker.Pos - defender.Pos) * (push ? -1 : 1);
        FlipX(defender.Pos - attacker.Pos, attacker.gameObject.GetComponent<SpriteRenderer>());
        return true;
    }

    protected override void Animate()
    {
        float percent;
        if (push)
        {
            switch (pushPullState)
            {
                case PushPullAnimationState.Approach:
                    percent = count * PushPullApproachRetreatSpeed;
                    UnitApproachPos(attacker, attackerBasePos, battleDirection, PushPullApproachDistance, percent);
                    if (count >= 1 / PushPullApproachRetreatSpeed)
                    {
                        count -= 1 / PushPullApproachRetreatSpeed;
                        SoundController.PlaySound(HitSFX, 1);
                        pushPullState = PushPullAnimationState.Move;
                    }
                    break;
                case PushPullAnimationState.Move:
                    percent = count * PushPullApproachRetreatSpeed;
                    UnitApproachPos(attacker, attackerBasePos + battleDirection * PushPullApproachDistance, -battleDirection, PushPullApproachDistance, percent);
                    percent = count * PushPullMoveSpeed;
                    UnitApproachPos(defender, defenderBasePos, battleDirection, 1, percent);
                    if (count >= 1 / PushPullMoveSpeed)
                    {
                        // Assume direction is valid (aka up/down/left/right)
                        defender.Pos += pushPullPosModifier;
                        pushPullState = PushPullAnimationState.Retreat;
                    }
                    break;
                case PushPullAnimationState.Retreat:
                    percent = count * PushPullApproachRetreatSpeed;
                    UnitApproachPos(attacker, attackerBasePos + battleDirection * PushPullApproachDistance, -battleDirection, PushPullApproachDistance, percent);
                    if (count >= 1 / PushPullApproachRetreatSpeed)
                    {
                        EndAnimation();
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (pushPullState)
            {
                case PushPullAnimationState.Approach:
                    percent = count * PushPullApproachRetreatSpeed;
                    UnitApproachPos(attacker, attackerBasePos, battleDirection, PushPullApproachDistance, percent);
                    if (count >= 1 / PushPullApproachRetreatSpeed)
                    {
                        count -= 1 / PushPullApproachRetreatSpeed;
                        SoundController.PlaySound(HitSFX, 1);
                        pushPullState = PushPullAnimationState.Move;
                    }
                    break;
                case PushPullAnimationState.Move:
                    percent = count * PushPullMoveSpeed;
                    UnitApproachPos(attacker, attackerBasePos + battleDirection * PushPullApproachDistance, -battleDirection, 1, percent);
                    UnitApproachPos(defender, defenderBasePos, -battleDirection, 1, percent);
                    if (count >= 1 / PushPullMoveSpeed)
                    {
                        // Assume direction is valid (aka up/down/left/right)
                        defender.Pos += pushPullPosModifier;
                        count -= 1 / PushPullMoveSpeed;
                        pushPullState = PushPullAnimationState.Retreat;
                    }
                    break;
                case PushPullAnimationState.Retreat:
                    percent = count * PushPullApproachRetreatSpeed;
                    UnitApproachPos(attacker, attackerBasePos + battleDirection * (PushPullApproachDistance - 1), -battleDirection, PushPullApproachDistance, percent);
                    if (count >= 1 / PushPullApproachRetreatSpeed)
                    {
                        attacker.Pos += pushPullPosModifier;
                        EndAnimation();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void UnitApproachPos(Unit unit, Vector3 unitBasePos, Vector3 direction, float moveDistance, float percent)
    {
        unit.transform.position = unitBasePos + direction * moveDistance * Mathf.Min(1, percent);
    }
}
