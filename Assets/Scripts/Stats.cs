using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Stats
{
    public int Strength;
    public int Endurance;
    public int Pierce;
    public int Armor;
    public int Precision;
    public int Evasion;
    public int MaxHP
    {
        get
        {
            return Endurance * 2;
        }
    }
    //public int HitChance(Stats other)
    //{
    //    return 100 - 5 * Mathf.Max(0, other.Evasion - Precision);
    //}
    //public int Damage(Stats other)
    //{
    //    return Mathf.Max(0, Strength - 2 * Mathf.Max(0, other.Armor - Pierce));
    //}
    public override string ToString()
    {
        return "End:" + Endurance.ToString().PadRight(2) + "Arm:" + Armor.ToString().PadRight(2) + "Eva:" + Evasion.ToString().PadRight(2) +
            "\nStr:" + Strength.ToString().PadRight(2) + "Pir:" + Pierce.ToString().PadRight(2) + "Pre:" + Precision.ToString().PadRight(2);
    }
    /*
     * Calculations:
     * HP = 2 * Endurance
     * Damage = Attacker's Strength - 2 * max(0, Defender's Armor - Attacker's Pierce)
     * Hit chance = 100 - 5 * max(0, Defender's Evasion - Attacker's Precision) %
     */
}
