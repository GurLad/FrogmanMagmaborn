using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Stats
{
    [Header("Growths (STR, END, PIR, ARM, PRE, EVA)")]
    public int[] Growths = new int[6];
    public int Strength
    {
        get
        {
            return statValues[0];
        }
        set
        {
            statValues[0] = value;
        }
    }
    public int Endurance
    {
        get
        {
            return statValues[1];
        }
        set
        {
            statValues[1] = value;
        }
    }
    public int Pierce
    {
        get
        {
            return statValues[2];
        }
        set
        {
            statValues[2] = value;
        }
    }
    public int Armor
    {
        get
        {
            return statValues[3];
        }
        set
        {
            statValues[3] = value;
        }
    }
    public int Precision
    {
        get
        {
            return statValues[4];
        }
        set
        {
            statValues[4] = value;
        }
    }
    public int Evasion
    {
        get
        {
            return statValues[5];
        }
        set
        {
            statValues[5] = value;
        }
    }
    public int MaxHP
    {
        get
        {
            return Endurance * 2;
        }
    }
    public int this[int index]
    {
        get
        {
            return statValues[index];
        }
        set
        {
            statValues[index] = value;
        }
    }
    private int[] statValues = new int[6] { 4, 4, 4, 4, 4, 4 }; // Default stats
    private int _sumGrowths = -1;
    private int sumGrowths
    {
        get
        {
            if (_sumGrowths < 0)
            {
                _sumGrowths = 0;
                for (int i = 0; i < Growths.Length; i++)
                {
                    _sumGrowths += Growths[i];
                }
            }
            return _sumGrowths;
        }
    }
    public static Stats operator+(Stats a, Stats b)
    {
        Stats stats = new Stats();
        for (int i = 0; i < 6; i++)
        {
            stats[i] = a[i] + b[i];
        }
        if (a.Growths != b.Growths)
        {
            Debug.LogWarning("Adding stats with different growths. This is probably a bug");
        }
        stats.Growths = a.Growths;
        return stats;
    }
    /// <summary>
    /// Returns the stats increased after numLevels level ups.
    /// Uses fixed values (no random level ups).
    /// </summary>
    /// <param name="numLevels">The number of times to level up.</param>
    /// <returns>Stat changes (for display and addition)</returns>
    public Stats GetLevelUp(int numLevels)
    {
        Stats result = new Stats();
        result.Growths = Growths;
        for (int i = 0; i < 6; i++)
        {
            result[i] = 0;
        }
        float currentStat = 0;
        for (int i = 0; i < 3 * numLevels; i++) // Three stats per level
        {
            // Current approach is very problematic. Consider: 9 1 9 1 9 1 growths with 1 level.
            currentStat += sumGrowths / (3.0f * numLevels);
            currentStat %= sumGrowths;
            Debug.Log("CurrentStat: " + currentStat + ", " + sumGrowths);
            float value = currentStat;
            int j = -1;
            do
            {
                value -= Growths[++j];
            } while (value > 0);
            result[j]++;
        }
        return result;
    }
    /// <summary>
    /// Returns the stats increased after a random level up.
    /// </summary>
    /// <returns>Stat changes (for display and addition)</returns>
    public Stats GetLevelUp()
    {
        Stats result = new Stats();
        result.Growths = Growths;
        for (int i = 0; i < 6; i++)
        {
            result[i] = 0;
        }
        for (int i = 0; i < 3; i++) // Three stats per level
        {
            int value = Random.Range(0, sumGrowths);
            int j = -1;
            do
            {
                value -= Growths[++j];
            } while (value > 0);
            result[j]++;
        }
        return result;
    }
    public override string ToString()
    {
        return "Str:" + Strength.ToString().PadRight(2) + "Pir:" + Pierce.ToString().PadRight(2) + "Pre:" + Precision.ToString().PadRight(2) +
            "\nEnd:" + Endurance.ToString().PadRight(2) + "Arm:" + Armor.ToString().PadRight(2) + "Eva:" + Evasion.ToString().PadRight(2);
    }
    /*
     * Calculations:
     * HP = 2 * Endurance
     * Damage = Attacker's Strength - 2 * max(0, Defender's Armor - Attacker's Pierce)
     * Hit chance = 100 - 5 * max(0, Defender's Evasion - Attacker's Precision) %
     */
}
