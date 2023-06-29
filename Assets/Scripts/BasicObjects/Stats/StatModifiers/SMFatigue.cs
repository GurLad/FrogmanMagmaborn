using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMFatigue : AStatModifier, IUnitListener
{
    public static int FatigueBarLength { get; } = 5;
    public int ArmorFatigue;
    public int EvasionFatigue;
    private Stats stats = Stats.Zero;

    public SMFatigue(Unit unit) : base(unit) { }

    public override Stats Modifier => stats;

    public void OnBlocked()
    {
        ArmorFatigue++;
        if (ArmorFatigue >= FatigueBarLength)
        {
            ArmorFatigue -= FatigueBarLength;
            stats.Armor--;
        }
    }

    public void OnDodged()
    {
        EvasionFatigue++;
        if (EvasionFatigue >= FatigueBarLength)
        {
            EvasionFatigue -= FatigueBarLength;
            stats.Evasion--;
        }
    }

    public void OnSpawn()
    {
        stats.Reset();
    }

    public void OnHit()
    {
        // Do nothing
    }

    public void OnMiss()
    {
        // Do nothing
    }

    public void OnDamaged()
    {
        // Do nothing
    }
}

// Concept: After blocking/dodging 5 times, the unit loses 1 armour/evasion
