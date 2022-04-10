using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniBattleStatsPanel : MonoBehaviour
{
    public PalettedSprite Panel;
    public Text Info;
    public InclinationIndicator Inclination;

    private void Reset()
    {
        Panel = GetComponent<PalettedSprite>();
        Info = GetComponentInChildren<Text>();
        Inclination = GetComponentInChildren<InclinationIndicator>();
    }

    public void DisplayBattleForecast(Unit origin, Unit target, bool reverse)
    {
        // Check if selecting the same unit...
        if (origin == target)
        {
            if (GameController.Current.InteractState == InteractState.Attack && reverse)
            {
                // ...to wait
                Panel.Palette = 3;
                Info.text = "\n Finish\n  move";
                Panel.gameObject.SetActive(true);
                if (Inclination != null)
                {
                    Inclination.gameObject.SetActive(false);
                }
                return;
            }
            else
            {
                // ...while moving (same as selecting nothing)
                if (reverse)
                {
                    origin = null;
                }
                else if (GameController.Current.InteractState == InteractState.Move)
                {
                    target = null;
                }
            }
        }
        else if (target != null && origin != null &&
                !origin.TheTeam.IsEnemy(target.TheTeam) &&
                ((reverse ? target : origin).HasSkill(Skill.Push) || (reverse ? target : origin).HasSkill(Skill.Pull)) &&
                (GameController.Current.InteractState == InteractState.Move || origin.Pos.TileDist(target.Pos) <= 1))
        { 
            // If origin has push/pull, use that as an ally action
            if (reverse)
            {
                Panel.Palette = (int)origin.TheTeam;
                string pushPull = target.HasSkill(Skill.Push) ? "Push" : "Pull";
                Info.text = "\n  " + pushPull + "\n" + new string(' ', (8 - origin.ToString().Length) / 2) + origin;
                Panel.gameObject.SetActive(true);
                if (Inclination != null)
                {
                    Inclination.gameObject.SetActive(false);
                }
                return;
            }
            else
            {
                target = origin;
            }
        }
        // Check if selecting nothing
        bool display = !reverse || (origin != null && origin.TheTeam.IsEnemy(target.TheTeam));
        if (!display)
        {
            Panel.gameObject.SetActive(false);
            return;
        }
        // Find many bools
        bool displayAttack = reverse || (origin != target && target != null && target.TheTeam.IsEnemy(origin.TheTeam));
        bool canAttack = target != null && (!reverse || GameController.Current.InteractState == InteractState.Move || origin.CanAttack(target) || GameController.Current.MarkerAtPos<AttackMarker>(origin.Pos));
        bool moveToCenter = !reverse && (target == null || (!target.TheTeam.IsEnemy(origin.TheTeam) && target != origin));
        // Show Info
        Panel.gameObject.SetActive(true);
        Panel.Palette = (int)origin.TheTeam;
        if (displayAttack) // Show battle preview
        {
            Info.text = origin.ToString() + "\n" + origin.AttackPreview(target, 2, canAttack);
        }
        else // Show unit name & Inclination
        {
            Info.text = origin.ToString() + "\n\n\nHP :" + origin.Health;
        }
        if (Inclination != null)
        {
            Inclination.gameObject.SetActive(true);
            Inclination.Display(origin, target);
        }
        // Move to center if there is only one panel
        if (!reverse)
        {
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, moveToCenter ? -20 : 0);
        }
    }

    public void DisplayMidBattleForecast(Unit origin, Unit target, bool reverse)
    {
        Panel.gameObject.SetActive(true);
        Panel.Palette = (int)origin.TheTeam;
        Info.text = origin.Name + "\n" + origin.AttackPreview(target, 2, origin.CanAttack(target));
        if (Inclination != null)
        {
            Inclination.gameObject.SetActive(true);
            Inclination.Display(origin, target);
        }
    }
}
