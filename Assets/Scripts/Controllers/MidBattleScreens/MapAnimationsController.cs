using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAnimationsController : MidBattleScreen
{
    public enum AnimationType { None, Movement, Battle, Delay, Push, Pull }
    private enum BattleAnimationState { AttackerAttacking, AttackerFinishingAttack, AttackerDelay, DefenderAttacking, DefenderFinishingAttack, DefenderDelay }
    private enum PushPullAnimationState { Approach, Move, Retreat }

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
    public AnimationType CurrentAnimation;
    [HideInInspector]
    public System.Action OnFinishAnimation;
    // Global animation vars
    private float count;
    // Movement animation vars
    private Unit currentUnit;
    private SpriteRenderer unitSpriteRenderer;
    private List<Vector2Int> path = new List<Vector2Int>();
    // Battle animation vars
    private float battleTrueFlashTime;
    private BattleAnimationState battleState;
    private MiniBattleStatsPanel attackerPanel;
    private MiniBattleStatsPanel defenderPanel;
    private AdvancedSpriteSheetAnimation missAnimation;
    // Battle, push & pull animations vars
    private Unit attacker;
    private Unit defender;
    private Vector3 attackerBasePos;
    private Vector3 defenderBasePos;
    private Vector3 battleDirection;
    // Push & pull animations vars
    private PushPullAnimationState pushPullState;
    private void Awake()
    {
        Current = this;
    }
    private void Update()
    {
        Time.timeScale = GameCalculations.GameSpeed(); // Double speed
        count += Time.deltaTime;
        float percent;
        switch (CurrentAnimation)
        {
            case AnimationType.None:
                break;
            case AnimationType.Movement:
                if (count >= 1 / WalkSpeed)
                {
                    count -= 1 / WalkSpeed;
                    SoundController.PlaySound(WalkSound);
                    FlipX(path[0] - currentUnit.Pos, unitSpriteRenderer);
                    currentUnit.Pos = path[0];
                    path.RemoveAt(0);
                    if (path.Count <= 0)
                    {
                        EndAnimation();
                    }
                }
                break;
            case AnimationType.Battle:
                percent = Mathf.Min(1, count * BattleSpeed);
                switch (battleState)
                {
                    case BattleAnimationState.AttackerAttacking:
                        attacker.transform.position = attackerBasePos + battleDirection * BattleMoveDistance * Mathf.Min(1, percent);
                        if (count >= 1 / BattleSpeed)
                        {
                            count -= 1 / BattleSpeed;
                            HandleDamage(attacker, defender, true);
                            battleState = BattleAnimationState.AttackerFinishingAttack;
                        }
                        break;
                    case BattleAnimationState.AttackerFinishingAttack:
                        attacker.transform.position = attackerBasePos + battleDirection * BattleMoveDistance * Mathf.Max(0, 1 - percent);
                        if (count >= battleTrueFlashTime && defender != null)
                        {
                            if (!defender.Statue)
                            {
                                defender.Moved = false;
                            }
                        }
                        if (count >= 1 / BattleSpeed)
                        {
                            count -= 1 / BattleSpeed;
                            battleState = BattleAnimationState.AttackerDelay;
                        }
                        break;
                    case BattleAnimationState.AttackerDelay:
                        if (count >= 1 / BattleSpeed)
                        {
                            count -= 1 / BattleSpeed;
                            if (defender != null && defender.CanAttack(attacker))
                            {
                                if (missAnimation != null)
                                {
                                    Destroy(missAnimation.gameObject);
                                }
                                battleState = BattleAnimationState.DefenderAttacking;
                            }
                            else
                            {
                                FinishBattle();
                            }
                        }
                        break;
                    case BattleAnimationState.DefenderAttacking:
                        defender.transform.position = defenderBasePos - battleDirection * BattleMoveDistance * Mathf.Min(1, percent);
                        if (count >= 1 / BattleSpeed)
                        {
                            count -= 1 / BattleSpeed;
                            HandleDamage(defender, attacker, false);
                            battleState = BattleAnimationState.DefenderFinishingAttack;
                        }
                        break;
                    case BattleAnimationState.DefenderFinishingAttack:
                        defender.transform.position = defenderBasePos - battleDirection * BattleMoveDistance * Mathf.Max(0, 1 - percent);
                        if (count >= battleTrueFlashTime && attacker != null)
                        {
                            attacker.Moved = false; // Attacker can't be a statue
                        }
                        if (count >= 1 / BattleSpeed)
                        {
                            count -= 1 / BattleSpeed;
                            battleState = BattleAnimationState.DefenderDelay;
                        }
                        break;
                    case BattleAnimationState.DefenderDelay:
                        if (count >= 1 / BattleSpeed)
                        {
                            if (missAnimation != null)
                            {
                                Destroy(missAnimation.gameObject);
                            }
                            FinishBattle();
                        }
                        break;
                    default:
                        break;
                }
                break;
            case AnimationType.Delay:
                if (count >= DelayTime)
                {
                    EndAnimation();
                }
                break;
            case AnimationType.Push:
                switch (pushPullState)
                {
                    case PushPullAnimationState.Approach:
                        percent = Mathf.Min(1, count * PushPullApproachRetreatSpeed);
                        UnitApproachPos(attacker, attackerBasePos, battleDirection, PushPullApproachDistance, percent);
                        if (count >= 1 / PushPullApproachRetreatSpeed)
                        {
                            count -= 1 / PushPullApproachRetreatSpeed;
                            pushPullState = PushPullAnimationState.Move;
                        }
                        break;
                    case PushPullAnimationState.Move:
                        percent = Mathf.Min(1, count * PushPullApproachRetreatSpeed);
                        UnitApproachPos(attacker, attackerBasePos + battleDirection * PushPullApproachDistance, -battleDirection, PushPullApproachDistance, percent);
                        percent = Mathf.Min(1, count * PushPullMoveSpeed);
                        UnitApproachPos(defender, defenderBasePos, battleDirection, 1, percent);
                        if (count >= 1 / PushPullMoveSpeed)
                        {
                            // Assume direction is valid (aka up/down/left/right)
                            defender.Pos += new Vector2Int((int)battleDirection.x, (int)battleDirection.y);
                            pushPullState = PushPullAnimationState.Retreat;
                        }
                        break;
                    case PushPullAnimationState.Retreat:
                        percent = Mathf.Min(1, count * PushPullApproachRetreatSpeed);
                        UnitApproachPos(attacker, attackerBasePos + battleDirection * PushPullApproachDistance, -battleDirection, PushPullApproachDistance, percent);
                        if (count >= 1 / PushPullApproachRetreatSpeed)
                        {
                            EndAnimation();
                        }
                        break;
                    default:
                        break;
                }
                break;
            case AnimationType.Pull:
                switch (pushPullState)
                {
                    case PushPullAnimationState.Approach:
                        percent = Mathf.Min(1, count * PushPullApproachRetreatSpeed);
                        UnitApproachPos(attacker, attackerBasePos, battleDirection, PushPullApproachDistance, percent);
                        if (count >= 1 / PushPullApproachRetreatSpeed)
                        {
                            count -= 1 / PushPullApproachRetreatSpeed;
                            pushPullState = PushPullAnimationState.Move;
                        }
                        break;
                    case PushPullAnimationState.Move:
                        percent = Mathf.Min(1, count * PushPullMoveSpeed);
                        UnitApproachPos(attacker, attackerBasePos + battleDirection * PushPullApproachDistance, -battleDirection, 1, percent);
                        UnitApproachPos(defender, defenderBasePos, -battleDirection, 1, percent);
                        if (count >= 1 / PushPullMoveSpeed)
                        {
                            // Assume direction is valid (aka up/down/left/right)
                            defender.Pos -= new Vector2Int((int)battleDirection.x, (int)battleDirection.y);
                            attacker.Pos -= new Vector2Int((int)battleDirection.x, (int)battleDirection.y);
                            count -= 1 / PushPullMoveSpeed;
                            pushPullState = PushPullAnimationState.Retreat;
                        }
                        break;
                    case PushPullAnimationState.Retreat:
                        percent = Mathf.Min(1, count * PushPullApproachRetreatSpeed);
                        UnitApproachPos(attacker, attackerBasePos + battleDirection * (PushPullApproachDistance - 1), -battleDirection, PushPullApproachDistance, percent);
                        if (count >= 1 / PushPullApproachRetreatSpeed)
                        {
                            EndAnimation();
                        }
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    private void UnitApproachPos(Unit unit, Vector3 unitBasePos, Vector3 direction, float moveDistance, float percent)
    {
        unit.transform.position = unitBasePos + direction * moveDistance * Mathf.Min(1, percent);
    }

    private void StartAnimation(AnimationType type)
    {
        CurrentAnimation = type;
        MidBattleScreen.Set(this, true);
    }

    private void EndAnimation()
    {
        CurrentAnimation = AnimationType.None;
        Time.timeScale = 1; // Remove double speed
        // Support for chaining animations & actions.
        count = 0;
        MidBattleScreen.Set(this, false);
        // Do a game-state check once before moving on to the next animation.
        if (GameController.Current.CheckGameState() != GameState.SideWon)
        {
            System.Action tempAction = OnFinishAnimation;
            OnFinishAnimation = null;
            tempAction?.Invoke();
        }
    }

    public void AnimateMovement(Unit unit, Vector2Int targetPos)
    {
        // Check if an animation is even needed
        if (OnFinishAnimation == null)
        {
            throw Bugger.Error("No OnFinishAnimation command - probably animated before assigning it.", false);
        }
        if (unit.Pos == targetPos)
        {
            OnFinishAnimation();
            return;
        }
        currentUnit = unit;
        if (path.Count > 0)
        {
            Bugger.Warning("Path isn't empty!");
            path.Clear();
        }
        path = unit.FindPath(targetPos);
        // Start animation
        unitSpriteRenderer = unit.gameObject.GetComponent<SpriteRenderer>();
        StartAnimation(AnimationType.Movement);
    }

    public void AnimateBattle(Unit attacking, Unit defending)
    {
        attacker = attacking;
        defender = defending;
        attackerBasePos = attacker.transform.position;
        defenderBasePos = defender.transform.position;
        battleDirection = (defenderBasePos - attackerBasePos).normalized;
        battleState = BattleAnimationState.AttackerAttacking;
        attackerPanel = InitPanel(GameController.Current.UIAttackerPanel, attacker, defender, false);
        defenderPanel = InitPanel(GameController.Current.UIDefenderPanel, defender, attacker, true);
        FlipX(defender.Pos - attacker.Pos, attacker.gameObject.GetComponent<SpriteRenderer>());
        FlipX(attacker.Pos - defender.Pos, defender.gameObject.GetComponent<SpriteRenderer>());
        StartAnimation(AnimationType.Battle);
    }

    public void AnimateDelay()
    {
        if (count != 0)
        {
            Bugger.Warning("Count isn't zero - it's " + count);
        }
        StartAnimation(AnimationType.Delay);
    }

    private MiniBattleStatsPanel InitPanel(MiniBattleStatsPanel panel, Unit attacking, Unit defending, bool reverse)
    {
        MiniBattleStatsPanel newPanel = Instantiate(panel.gameObject, panel.transform.parent.parent).GetComponent<MiniBattleStatsPanel>();
        newPanel.DisplayMidBattleForecast(attacking, defending, false);
        RectTransform rectTransform = newPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = BattleBasePanelPosition.anchorMin;
        rectTransform.anchorMax = BattleBasePanelPosition.anchorMax;
        rectTransform.sizeDelta = BattleBasePanelPosition.sizeDelta;
        int size = GameController.Current.MapSize.x / 2;
        // Calculate x pos - harder than it sounds
        float posX;
        int sign;
        if (Mathf.Sign(attacking.Pos.x - size) == Mathf.Sign(defending.Pos.x - size)) // Good
        {
            posX = (Mathf.Abs(attacking.Pos.x - size) < Mathf.Abs(defending.Pos.x - size) ? attacking.Pos.x - size : defending.Pos.x - size) + 0.5f;
            sign = posX > 0 ? -1 : 1;
        }
        else // Oof. Let's prioritize the attacker (not attacking), so it will be the same for both sides.
        {
            posX = (attacker.Pos.x * Mathf.Sign(attacker.Pos.x - size) < defender.Pos.x * Mathf.Sign(attacker.Pos.x - size) ? attacker.Pos.x - size : defender.Pos.x - size) + 0.5f;
            sign = -(int)Mathf.Sign(attacker.Pos.x - size);
        }
        float posY = Mathf.Clamp((attacking.Pos.y + defending.Pos.y) / 2f + 0.5f, 3, GameController.Current.MapSize.y - 3);
        rectTransform.anchoredPosition = new Vector2(
            posX * GameController.Current.TileSize * 16 + (BattleBasePanelPosition.sizeDelta.x / 2 + 16) * sign,
            -posY * GameController.Current.TileSize * 16);
        return newPanel;
    }

    private bool? HandleDamage(Unit attacking, Unit defending, bool attackerAttack)
    {
        bool? result = attacking.Attack(defending);
        switch (result)
        {
            case true:
                // Play sound for hit
                int damage = attacking.GetDamage(defending);
                if (damage == 0)
                {
                    SoundController.PlaySound(NoDamageSFX, 1);
                }
                else
                {
                    SoundController.PlaySound(HitSFX, 1.5f - (float)damage / defending.Stats.MaxHP);
                    battleTrueFlashTime = BattleFlashTime / (1.5f - (float)damage / defending.Stats.MaxHP);
                    if (!defending.Statue)
                    {
                        defending.Moved = true; // "Flash"
                    }
                }
                break;
            case false:
                // Show animation for miss
                SoundController.PlaySound(MissSFX, 1);
                missAnimation = Instantiate(BattleMissAnimation.gameObject).GetComponent<AdvancedSpriteSheetAnimation>();
                missAnimation.transform.position = attackerAttack ? defenderBasePos : attackerBasePos;
                missAnimation.transform.position += new Vector3(0, 0, -0.5f);
                missAnimation.Start();
                missAnimation.Activate(0);
                missAnimation.gameObject.SetActive(true);
                break;
            case null:
                // Destroy sprite for dead
                SoundController.PlaySound(HitSFX, 0.5f);
                break;
        }
        // Update display
        if (attacker != null && defender != null)
        {
            attackerPanel.DisplayMidBattleForecast(attacker, defender, false);
            defenderPanel.DisplayMidBattleForecast(defender, attacker, true);
        }
        else
        {
            if (attacker != null)
            {
                attackerPanel.DisplayMidBattleForecast(attacker, attacker, false);
                defenderPanel.DisplayMidBattleForecast(attacker, attacker, true);
            }
            else
            {
                attackerPanel.DisplayMidBattleForecast(defender, defender, true);
                defenderPanel.DisplayMidBattleForecast(defender, defender, false);
            }
        }
        return result;
    }

    private void FinishBattle()
    {
        Destroy(attackerPanel.gameObject);
        Destroy(defenderPanel.gameObject);
        EndAnimation();
    }

    private void FlipX(Vector2Int direction, SpriteRenderer unitRenderer)
    {
        unitRenderer.flipX = direction.x != 0 ? (direction.x > 0 ? true : false) : unitRenderer.flipX;
    }
}
