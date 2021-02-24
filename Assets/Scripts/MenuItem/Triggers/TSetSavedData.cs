using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSetSavedData : Trigger
{
    public string FileName;
    public string DataName;
    public SaveMode SaveMode;
    public Text DisplayLabel;
    public List<string> Values;
    private int current;

    private void Start()
    {
        current = FileName != "" ? SavedData.Load<int>(FileName, DataName) : SavedData.Load(DataName, 0, SaveMode);
        DisplayLabel.text = Values[current];
    }

    public override void Activate()
    {
        current++;
        current %= Values.Count;
        if (FileName != "")
        {
            SavedData.Save(FileName, DataName, current);
        }
        else
        {
            SavedData.Save(DataName, current, SaveMode);
        }
        DisplayLabel.text = Values[current];
    }
}