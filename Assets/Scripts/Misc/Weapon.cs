using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Weapon
{
    private const int BASE_HIT = 80;
    public string Name;
    [SerializeField]
    private int HitStat;
    public int Damage;
    public int Weight;
    public int Range = 1; // Only abilities can modify
    [HideInInspector]
    public int Hit
    {
        get
        {
            return BASE_HIT + 10 * HitStat;
        }
    }
    public Weapon(int level)
    {
        int[] stats = new int[3]; // Hit, damage, weight
        Weight = level;
        if (level != 0)
        {
            int minValue = Random.Range(-level - 1, 1);
            int maxValue = level - minValue;
            for (int i = minValue; i < maxValue; i++)
            {
                stats[Random.Range(0, stats.Length)] += (int)Mathf.Sign(i);
            }
        }
        HitStat = stats[0];
        Damage = stats[1];
        Weight -= stats[2];
    }
    public override string ToString()
    {
        return Name + "\nPOW:" + Damage.ToString().PadRight(4) + "ACC:" + Hit.ToString().PadRight(4) + "WT: " + Weight.ToString().PadRight(4) + "RNG:" + Range;
    }
}
