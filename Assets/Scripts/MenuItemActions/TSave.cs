using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSave : Trigger
{
    public SaveMode SaveMode;

    public override void Activate()
    {
        SavedData.SaveAll(SaveMode);
    }
}
