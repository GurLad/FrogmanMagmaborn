using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMTake : AStatModifierPos
{
    public SMTake(Unit unit) : base(unit) { }

    public override Stats Modifier
    {
        get
        {
            Stats stats = new Stats();
            stats.Armor = !unit.TheTeam.IsMainPlayerTeam() ? -unit.CountAdjacentAllies(pos) : 0;
            return stats;
        }
    }
}
