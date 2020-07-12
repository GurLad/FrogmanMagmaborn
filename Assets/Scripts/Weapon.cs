using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Weapon
{
    private const int BASE_HIT = 80;
    public int Hit;
    public int Damage;
    public int Range;
    public Weapon(int level)
    {
        int minValue = Random.Range(-level - 1, 1);
        int maxValue = level - minValue;
        int[] stats = new int[3]; // Hit, damage, Range
        for (int i = minValue; i < maxValue; i++)
        {
            stats[Random.Range(0, stats.Length)] += (int)Mathf.Sign(i);
        }
        int leftover = stats[2] % 3;
        for (int i = 0; i < Mathf.Abs(leftover); i++)
        {
            stats[Random.Range(0, stats.Length - 1)] += (int)Mathf.Sign(leftover);
        }
        Hit = BASE_HIT + 5 * stats[0];
        Damage = stats[1];
        Range = 1 + stats[2] / 3;
    }
    public override string ToString()
    {
        return "Weapon name\nPOW:" + Damage.ToString().PadRight(2) + "HIT:" + Hit.ToString().PadRight(3) + "RNG:" + Range;
    }
}
