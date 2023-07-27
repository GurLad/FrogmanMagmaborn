using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MABattle : MapAnimation
{
    private enum BattleAnimationState { AttackerAttacking, AttackerFinishingAttack, AttackerDelay, DefenderAttacking, DefenderFinishingAttack, DefenderDelay }

    public float BattleSpeed;
    public float BattleFlashTime;
    public float BattleMoveDistance;
    public AdvancedSpriteSheetAnimation BattleMissAnimation;
    public RectTransform BattleBasePanelPosition;
    public AudioClip HitSFX;
    public AudioClip MissSFX;
    public AudioClip NoDamageSFX;
    // Battle animation vars
    private float battleAttackerRandomResult;
    private float battleDefenderRandomResult;
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

    public bool Init(System.Action onFinishAnimation,
        float battleSpeed,
        float battleFlashTime,
        float battleMoveDistance,
        AdvancedSpriteSheetAnimation battleMissAnimation,
        RectTransform battleBasePanelPosition,
        AudioClip hitSFX, AudioClip missSFX, AudioClip noDamageSFX,
        Unit attacking, Unit defending, float attackerRandomResult, float defenderRandomResult)
    {
        // Constructor
        OnFinishAnimation = onFinishAnimation;
        BattleSpeed = battleSpeed;
        BattleFlashTime = battleFlashTime;
        BattleMoveDistance = battleMoveDistance;
        BattleMissAnimation = battleMissAnimation;
        BattleBasePanelPosition = battleBasePanelPosition;
        HitSFX = hitSFX;
        MissSFX = missSFX;
        NoDamageSFX = noDamageSFX;
        // Init
        attacker = attacking;
        defender = defending;
        battleAttackerRandomResult = attackerRandomResult;
        battleDefenderRandomResult = defenderRandomResult;
        attackerBasePos = attacker.transform.position;
        defenderBasePos = defender.transform.position;
        battleDirection = (defenderBasePos - attackerBasePos).normalized;
        battleState = BattleAnimationState.AttackerAttacking;
        attackerPanel = InitPanel(GameController.Current.GameUIController.UIAttackerPanel, attacker, defender, false);
        defenderPanel = InitPanel(GameController.Current.GameUIController.UIDefenderPanel, defender, attacker, true);
        FlipX(defender.Pos - attacker.Pos, attacker.gameObject.GetComponent<SpriteRenderer>());
        FlipX(attacker.Pos - defender.Pos, defender.gameObject.GetComponent<SpriteRenderer>());
        return init = true;
    }

    protected override void Animate()
    {
        float percent = count * BattleSpeed;
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
                    if (missAnimation != null)
                    {
                        Destroy(missAnimation.gameObject);
                    }
                    if (defender != null && defender.Health > 0 && defender.CanAttack(attacker))
                    {
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
        Vector2 pos;
        Vector2Int screenTileSize = new Vector2Int(6, 7);
        int sign;
        if (Mathf.Abs(attacking.Pos.x - defending.Pos.x) > screenTileSize.x || Mathf.Abs(attacking.Pos.y - defending.Pos.y) > screenTileSize.y) // Siege weapon - put it near the target
        {
            // For siege weapons, 99% of the time the defender won't counter, so it's more important to know where they are and not the attacker
            pos.x = defender.Pos.x - size + 0.5f;
            sign = pos.x > 0 ? -1 : 1;
            pos.y = defender.Pos.y + 0.5f;
        }
        else if (Mathf.Sign(attacking.Pos.x - size) == Mathf.Sign(defending.Pos.x - size)) // Both attackers are on the same side - prioritize the one closer to the centre
        {
            pos.x = (Mathf.Abs(attacking.Pos.x - size) < Mathf.Abs(defending.Pos.x - size) ? attacking.Pos.x - size : defending.Pos.x - size) + 0.5f;
            sign = pos.x > 0 ? -1 : 1;
            pos.y = (attacking.Pos.y + defending.Pos.y) / 2f + 0.5f;
        }
        else // Attackers on different sides - prioritize the one closer to the centre again, but use the opposite direction (if the one closer to the centre is too close to the edge, the dist is at least 1 and option 1 would be taken)
        {
            pos.x = (Mathf.Abs(attacking.Pos.x - size) < Mathf.Abs(defending.Pos.x - size) ? attacking.Pos.x - size : defending.Pos.x - size) + 0.5f;
            sign = pos.x > 0 ? 1 : -1;
            pos.y = (attacking.Pos.y + defending.Pos.y) / 2f + 0.5f;
        }
        pos.y = Mathf.Clamp(pos.y, 3, GameController.Current.MapSize.y - 3);
        rectTransform.anchoredPosition = new Vector2(
            pos.x * GameController.Current.TileSize * 16 + (BattleBasePanelPosition.sizeDelta.x / 2 + 16) * sign,
            -pos.y * GameController.Current.TileSize * 16);
        return newPanel;
    }

    private bool? HandleDamage(Unit attacking, Unit defending, bool attackerAttack)
    {
        bool? result = attacking.Attack(defending, attacker == attacking ? battleAttackerRandomResult : battleDefenderRandomResult);
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
                    SoundController.PlaySound(HitSFX, 1.5f - (float)damage / defending.Stats.Base.MaxHP);
                    battleTrueFlashTime = BattleFlashTime / (1.5f - (float)damage / defending.Stats.Base.MaxHP);
                    if (!defending.Statue)
                    {
                        defending.Moved = true; // "Flash"
                    }
                }
                break;
            case false:
                // Show animation for miss
                SoundController.PlaySound(MissSFX, 1);
                missAnimation = CreateAnimationOnUnit(attackerAttack ? defender : attacker, BattleMissAnimation);
                missAnimation.Activate(0);
                break;
            case null:
                // Destroy sprite for dead
                SoundController.PlaySound(HitSFX, 0.5f);
                battleTrueFlashTime = BattleFlashTime * 2; // Flash in case it has a death quote
                if (!defending.Statue)
                {
                    defending.Moved = true; // "Flash"
                }
                break;
        }
        // Update display
        attackerPanel.DisplayMidBattleForecast(attacker, defender, false);
        defenderPanel.DisplayMidBattleForecast(defender, attacker, true);
        return result;
    }

    private void FinishBattle()
    {
        Destroy(attackerPanel.gameObject);
        Destroy(defenderPanel.gameObject);
        EndAnimation();
    }
}
