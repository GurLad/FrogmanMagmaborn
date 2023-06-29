using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class UnitStats
{
    private enum VisibilityOptions { All, VisibleOnly, InvisibleOnly }

    public Stats Base;
    public Stats VisibleModifiers => GetModifiers(null, unit.Pos, VisibilityOptions.VisibleOnly);
    public Stats InvisibleModifiers => GetModifiers(null, unit.Pos, VisibilityOptions.InvisibleOnly);
    public Stats Total => AtPos(unit.Pos);
    private Unit unit;
    [SerializeField]
    private List<AStatModifier> modifiers = new List<AStatModifier>();

    public UnitStats(Unit unit)
    {
        this.unit = unit;
        Base = new Stats();
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

    private Stats GetModifiers(Unit target, Vector2Int pos, VisibilityOptions visibilityOptions = VisibilityOptions.All)
    {
        List<AStatModifier> active = visibilityOptions switch
        {
            VisibilityOptions.All => modifiers,
            VisibilityOptions.VisibleOnly => modifiers.FindAll(a => a.Visible),
            VisibilityOptions.InvisibleOnly => modifiers.FindAll(a => !a.Visible),
            _ => throw Bugger.FMError("Impossible")
        };
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
        sum.Growths = Base.Growths;
        active.ForEach(a => { Stats mod = a.Modifier; mod.Growths = sum.Growths; sum += mod; });
        return sum;
    }
}

