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
    public int HitChance(Stats other)
    {
        return 100 - 5 * Mathf.Max(0, other.Evasion - Precision);
    }
    public int Damage(Stats other)
    {
        return Mathf.Max(0, Strength - 2 * Mathf.Max(0, Pierce - other.Armor));
    }
    /*
     * Calculations:
     * HP = 2 * Endurance
     * Damage = Attacker's Strength - 2 * max(0, Attacker's Pierce - Defender's Armor)
     * Hit chance = 100 - 5 * max(0, Defender's Evasion - Attacker's Precision) %
     */
}
