using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMWeapon : AStatModifier
{
    public override bool Visible => false;

    public SMWeapon(Unit unit) : base(unit) { }

    public override Stats Modifier
    {
        get
        {
            Stats stats = Stats.Zero;
            stats.Strength += unit.Weapon.Damage;
            stats.Evasion -= unit.Weapon.Weight;
            stats.Precision += unit.Weapon.Hit / 10;
            return stats;
        }
    }
}
