using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitControls : MonoBehaviour
{
    private void Awake()
    {
        if (!SavedData.HasKey("InitControls", SaveMode.Global))
        {
            SavedData.Save("InitControls", 1, SaveMode.Global);
            Control.SetButton(Control.CB.A, KeyCode.Keypad4);
            Control.SetButton(Control.CB.B, KeyCode.Keypad8);
            Control.SetButton(Control.CB.Select, KeyCode.Keypad7);
            Control.SetButton(Control.CB.Start, KeyCode.Keypad6);
            Control.SetAxis(Control.Axis.X, KeyCode.RightArrow, KeyCode.LeftArrow);
            Control.SetAxis(Control.Axis.Y, KeyCode.UpArrow, KeyCode.DownArrow);
        }
    }
}
