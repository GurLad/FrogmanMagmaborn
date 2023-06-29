using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats
{
    public Stats Base => baseStats;
    public Stats Modifiers => AtPos(unit.Pos);
    private Unit unit;
    [SerializeField]
    private Stats baseStats;
    [SerializeField]
    private List<AStatModifier> modifiers;

    public UnitStats(Unit unit)
    {
        this.unit = unit;
    }

    public Stats Against(Unit target, Vector2Int? pos)
    {
        return Base + GetModifiers(target, pos ?? unit.Pos);
    }

    public Stats AtPos(Vector2Int pos)
    {
        return Base + GetModifiers(null, pos);
    }
    
    private Stats GetModifiers(Unit target, Vector2Int pos)
    {
        modifiers.FindAll(a => a is AStatModifierPos).ForEach(a => ((AStatModifierPos)a).SetPos(pos));
        modifiers.FindAll(a => a is AStatModifierBattle).ForEach(a => ((AStatModifierBattle)a).SetTarget(target));
        Stats sum = new Stats();
        modifiers.FindAll(a => target != null || !(a is AStatModifierBattle)).ForEach(a => sum += a.Modifier);
        return sum;
    }
}

