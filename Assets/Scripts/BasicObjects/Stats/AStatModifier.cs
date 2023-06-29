using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AStatModifier
{
    protected Unit unit;

    public AStatModifier(Unit unit)
    {
        this.unit = unit;
    }

    public abstract Stats Modifier { get; }
}
