using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BARangedAttack : BattleAnimation
{
    private const float SPEED = 4;
    private float projectileTargetPos;
    private GameObject currentProjectile;
    private Vector3 currentPorjectilePos;

    private void Update()
    {
        if (currentProjectile != null)
        {
            currentPorjectilePos.x -= Time.deltaTime * SPEED * ThisCombatant.LookingLeftSign;
            currentProjectile.transform.position = currentPorjectilePos;
            if (currentPorjectilePos.x * ThisCombatant.LookingLeftSign <= projectileTargetPos * ThisCombatant.LookingLeftSign)
            {
                Destroy(currentProjectile);
                BattleAnimationController.HandleDamage(ThisCombatant, OtherCombatant);
                Finish();
            }
        }
    }

    public override void FinishedAnimation(int id, string name)
    {
        base.FinishedAnimation(id, name);
        switch (name)
        {
            case "AttackRangeStart":
                GameObject projectileSource = BattleAnimationController.ClassAnimations.Find(a => a.Name == ThisCombatant.Unit.Class).Projectile;
                currentProjectile = Instantiate(projectileSource, ThisCombatant.Object.transform);
                currentProjectile.SetActive(true);
                Vector3 pos = projectileSource.transform.localPosition;
                pos.x *= ThisCombatant.LookingLeft ? 1 : -1;
                projectileTargetPos = OtherCombatant.Object.transform.position.x + ThisCombatant.LookingLeftSign;
                currentProjectile.transform.localPosition = pos;
                currentProjectile.GetComponent<PalettedSprite>().Palette = (int)ThisCombatant.Unit.TheTeam;
                currentPorjectilePos = currentProjectile.transform.position;
                currentProjectile.GetComponent<SpriteRenderer>().flipX = !ThisCombatant.LookingLeft;
                ThisCombatant.Animation.Activate("AttackRangeEnd");
                break;
            case "AttackRangeEnd":
                ThisCombatant.Animation.Activate("Idle");
                break;
            default:
                break;
        }
    }

    public override void Init(BattleAnimationController.CombatantData thisCombatant, BattleAnimationController.CombatantData otherCombatant, BattleAnimationController battleAnimationController)
    {
        base.Init(thisCombatant, otherCombatant, battleAnimationController);
        if (ThisCombatant.Unit.CanAttack(OtherCombatant.Unit))
        {
            // Fix looking left for backstabs (teleport)
            ThisCombatant.LookingLeft = !OtherCombatant.LookingLeft;
            ThisCombatant.Animation.Activate("AttackRangeStart");
        }
        else
        {
            Finish();
        }
    }
}
