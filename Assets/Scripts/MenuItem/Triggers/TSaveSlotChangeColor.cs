using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSaveSlotChangeColor : Trigger
{
    public Text Text;
    private int palette;

    private void Reset()
    {
        Text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        palette = SavedData.Load("SaveSlotPalette", -1) >= 0 ? SavedData.Load("SaveSlotPalette", -1) : (int)StaticGlobals.MainPlayerTeam;
        Text.text = "Color: " + "@".ToColoredString(palette);
    }

    public override void Activate()
    {
        SavedData.Save("SaveSlotPalette", palette = (palette + 1) % 4);
        Text.text = "Color: " + "@".ToColoredString(palette);
    }
}
