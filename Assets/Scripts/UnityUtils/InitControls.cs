using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitControls : MonoBehaviour
{
    public Text InitA;
    public Text InitB;
    public Text InitStart;
    private void Awake()
    {
        if (!SavedData.HasKey("InitControls", SaveMode.Global))
        {
            SavedData.Save("InitControls", 1, SaveMode.Global);
            Control.SetButton(Control.CB.A, KeyCode.X);
            Control.SetButton(Control.CB.B, KeyCode.Z);
            Control.SetButton(Control.CB.Select, KeyCode.A);
            Control.SetButton(Control.CB.Start, KeyCode.Return);
            Control.SetAxis(Control.Axis.X, KeyCode.RightArrow, KeyCode.LeftArrow);
            Control.SetAxis(Control.Axis.Y, KeyCode.UpArrow, KeyCode.DownArrow);
            InitA.gameObject.SetActive(true);
            InitB.gameObject.SetActive(true);
            InitStart.text = InitStart.text.Replace("Start", "Enter");
        }
    }
}
