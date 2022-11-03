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
        Str.text = ":" + origin.BattleStatsStr;
        End.text = ":" + origin.BattleStatsEnd;
        Pir.text = ":" + origin.BattleStatsPir;
        Arm.text = ":" + origin.BattleStatsArm;
        Pre.text = ":" + origin.BattleStatsPre;
        Eva.text = ":" + origin.BattleStatsEva;
        int armMod;
        if (includeArmorMod && (armMod = origin.GetArmorModifier(origin.Pos)) != 0)
        {
            Arm.text += (armMod > 0 ? "+" : "") + armMod;
        }
    }
}
