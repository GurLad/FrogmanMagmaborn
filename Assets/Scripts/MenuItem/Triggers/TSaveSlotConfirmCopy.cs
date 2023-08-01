using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSaveSlotConfirmCopy : Trigger
{
    public TSaveSlotUpdate SaveSlotUpdate;
    private int _target;
    public int Target { private get => _target; set => SaveSlotUpdate.NewSlot = _target = value; }

    public override void Activate()
    {
        // TBA
    }
}
