using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMInclinationBonus : AStatModifierBattle
{
    public SMInclinationBonus(Unit unit) : base(unit) { }

    public override Stats Modifier
    {
        get
        {
            Stats stats = new Stats();
            if (unit.EffectiveAgainst(target))
            {
                stats[(int)unit.Inclination * 2] += 2;
            }
            return stats;
        }
    }
}
