using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMGive : AStatModifierPos
{
    public SMGive(Unit unit) : base(unit) { }

    public override Stats Modifier
    {
        get
        {
            Stats stats = Stats.Zero;
            stats.Armor = unit.TheTeam.IsMainPlayerTeam() ? unit.CountAdjacentAllies(pos) : 0;
            return stats;
        }
    }
}
