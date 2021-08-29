﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSetDifficulty : Trigger
{
    public Difficulty Difficulty;
    public override void Activate()
    {
        SavedData.Save("Knowledge", "UpgradeDifficulty", (int)Difficulty);
        SavedData.SaveAll(SaveMode.Slot);
    }
}
