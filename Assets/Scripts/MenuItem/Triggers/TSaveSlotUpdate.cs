using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSaveSlotUpdate : Trigger
{
    public SaveSlot SaveSlot;
    public int NewSlot = -1;

    public override void Activate()
    {
        SaveSlot.Select(NewSlot);
    }
}
