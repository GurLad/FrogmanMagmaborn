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
        Application.targetFrameRate = 60; // To prevent my laptop from burning itself trying to run the game at 700 FPS
        // Create a new slot. Move this to a slot switcher once I add that option.
        SavedData.CreateFile("Knowledge", SaveFileType.File);
        SavedData.CreateFile("ConversationData", SaveFileType.File);
        if (!SavedData.HasKey("InitControls", SaveMode.Global))
        {
            SavedData.Save("InitControls", 1, SaveMode.Global);
            Control.SetButton(Control.CB.A, KeyCode.X);
            Control.SetButton(Control.CB.B, KeyCode.Z);
            Control.SetButton(Control.CB.Select, KeyCode.C);
            Control.SetButton(Control.CB.Start, KeyCode.Return);
            Control.SetAxis(Control.Axis.X, KeyCode.RightArrow, KeyCode.LeftArrow);
            Control.SetAxis(Control.Axis.Y, KeyCode.UpArrow, KeyCode.DownArrow);
            if (InitA != null)
            {
                InitA.gameObject.SetActive(true);
                InitB.gameObject.SetActive(true);
                InitStart.text = InitStart.text.Replace("Start", "Enter");
            }
        }
    }
}
