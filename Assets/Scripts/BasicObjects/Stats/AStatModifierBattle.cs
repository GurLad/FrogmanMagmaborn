using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AStatModifierBattle : AStatModifier
{
    protected Unit target;

    public AStatModifierBattle(Unit unit) : base(unit) {}

    public void SetTarget(Unit target)
    {
        this.target = target;
    }
}
