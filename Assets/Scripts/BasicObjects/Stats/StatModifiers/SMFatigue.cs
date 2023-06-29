using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMFatigue : AStatModifier
{
    public static int FatigueBarLength { get; } = 5;
    public int ArmorFatigue;
    public int EvasionFatigue;
    private Stats stats;

    public SMFatigue(Unit unit) : base(unit) { }

    public override Stats Modifier => stats;

    public void OnBlock()
    {
        ArmorFatigue++;
        if (ArmorFatigue >= FatigueBarLength)
        {
            ArmorFatigue -= FatigueBarLength;
            stats.Armor--;
        }
    }

    public void OnDodge()
    {
        EvasionFatigue++;
        if (EvasionFatigue >= FatigueBarLength)
        {
            EvasionFatigue -= FatigueBarLength;
            stats.Evasion--;
        }
    }

    public void OnMapClear()
    {
        stats.Reset();
    }
}

// Concept: After blocking/dodging 5 times, the unit loses 1 armour/evasion
