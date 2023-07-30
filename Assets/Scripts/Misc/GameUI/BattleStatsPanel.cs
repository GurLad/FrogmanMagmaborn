using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStatsPanel : MonoBehaviour
{
    public Text Str;
    public Text End;
    public Text Pir;
    public Text Arm;
    public Text Pre;
    public Text Eva;

    public void Display(Unit origin, Stats mod = null)
    {
        Display(origin, mod == null);
        if (mod != null)
        {
            Str.text += "+" + mod.Strength;
            End.text += "+" + mod.MaxHP;
            Pir.text += "+" + mod.Pierce;
            Arm.text += "+" + mod.Armor;
            Pre.text += "+" + mod.Precision * 10;
            Eva.text += "+" + mod.Evasion * 10;
        }
    }

    public void Display(Unit origin, bool includeArmorMod)
    {
        Stats baseStats = origin.Stats.Base + origin.Stats.InvisibleModifiers;
        Str.text = ":" + baseStats.Strength;
        End.text = ":" + origin.Stats.Base.MaxHP;
        Pir.text = ":" + baseStats.Pierce;
        Arm.text = ":" + baseStats.Armor;
        Pre.text = ":" + (baseStats.GetHit() - 40);
        Eva.text = ":" + (baseStats.GetAvoid() - 40);
        if (includeArmorMod)
        {
            Stats visibleMods = origin.Stats.VisibleModifiers;
            Str.text += ProcessMod(visibleMods.Strength);
            Pir.text += ProcessMod(visibleMods.Pierce);
            Arm.text += ProcessMod(visibleMods.Armor);
            Pre.text += ProcessMod(visibleMods.GetHit());
            Eva.text += ProcessMod(visibleMods.GetAvoid());
        }
    }

    private string ProcessMod(int mod)
    {
        return mod != 0 ? ((mod > 0 ? "+" : "") + mod) : "";
    }
}
