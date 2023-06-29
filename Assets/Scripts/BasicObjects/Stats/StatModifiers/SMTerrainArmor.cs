using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMTerrainArmor : AStatModifierPos
{
    public SMTerrainArmor(Unit unit) : base(unit) { }

    public override Stats Modifier
    {
        get
        {
            Tile tile = GameController.Current.Map[pos.x, pos.y];
            Stats stats = Stats.Zero;
            stats.Armor =
                ((unit.Flies && !tile.High) ? 0 :
                (unit.HasSkill(Skill.NaturalCover) ? Mathf.Abs(tile.ArmorModifier) : tile.ArmorModifier));
            return stats;
        }
    }
}
