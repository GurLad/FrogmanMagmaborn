using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats
{
    public Stats Base => baseStats;
    public Stats VisibleModifiers => GetModifiers(null, unit.Pos, true);
    public Stats Total => AtPos(unit.Pos);
    private Unit unit;
    [SerializeField]
    private Stats baseStats;
    [SerializeField]
    private List<AStatModifier> modifiers = new List<AStatModifier>();

    public UnitStats(Unit unit)
    {
        this.unit = unit;
        baseStats = new Stats();
    }

    public Stats Against(Unit target, Vector2Int? pos)
    {
        return Base + GetModifiers(target, pos ?? unit.Pos);
    }

    public Stats AtPos(Vector2Int pos)
    {
        return Base + GetModifiers(null, pos);
    }

    public void AddStatModifier<T>(T modifier) where T : AStatModifier
    {
        modifiers.Add(modifier);
    }

    public void IncreaseBaseStats(Stats amount)
    {
        baseStats += amount;
    }

    private Stats GetModifiers(Unit target, Vector2Int pos, bool visibleOnly = false)
    {
        List<AStatModifier> active = visibleOnly ? modifiers.FindAll(a => a.Visible) : modifiers;
        if (target == null)
        {
            active = active.FindAll(a => !(a is AStatModifierBattle));
        }
        else
        {
            active.FindAll(a => a is AStatModifierBattle).ForEach(a => ((AStatModifierBattle)a).SetTarget(target));
        }
        active.FindAll(a => a is AStatModifierPos).ForEach(a => ((AStatModifierPos)a).SetPos(pos));
        Stats sum = Stats.Zero;
        active.ForEach(a => sum += a.Modifier);
        sum.Growths = Base.Growths;
        return sum;
    }
}

