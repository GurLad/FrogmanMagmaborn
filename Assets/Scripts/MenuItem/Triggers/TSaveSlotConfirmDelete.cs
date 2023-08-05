using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSaveSlotConfirmDelete : Trigger
{
    public override void Activate()
    {
        SavedData.DeleteAll(SaveMode.Slot, -1);
        SavedData.SaveAll(SaveMode.Slot, -1, true);
    }
}
