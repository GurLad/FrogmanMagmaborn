using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AStatModifierPos : AStatModifier
{
    protected Vector2Int pos;

    public AStatModifierPos(Unit unit) : base(unit) { }

    public void SetPos(Vector2Int pos)
    {
        this.pos = pos;
    }
}
