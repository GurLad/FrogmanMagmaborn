using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFlash : MonoBehaviour
{
    public const float TIME = 0.1f;
    private Unit unit;
    private bool moved;
    private float count;

    public void Init(Unit target)
    {
        unit = target;
        moved = unit.Moved;
        count = 0;
    }

    private void Update()
    {
        count += Time.deltaTime;
        if (count >= TIME * 4)
        {
            unit.Moved = moved;
            Destroy(this);
            return;
        }
        if ((count >= TIME && count <= TIME * 2) || (count >= TIME * 3))
        {
            unit.Moved = true;
        }
        else
        {
            unit.Moved = false;
        }
    }
}
