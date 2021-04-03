using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSetSingleIntSavedData : Trigger
{
    public string FileName;
    public string DataName;
    public SaveMode SaveMode;
    public int Data;

    public override void Activate()
    {
        if (FileName != "")
        {
            SavedData.Save(FileName, DataName, Data);
            SavedData.SaveAll(SaveMode.Slot);
        }
        else
        {
            SavedData.Save(DataName, Data, SaveMode);
            SavedData.SaveAll(SaveMode);
        }
    }
}
