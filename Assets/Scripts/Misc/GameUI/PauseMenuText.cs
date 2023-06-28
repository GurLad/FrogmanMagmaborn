﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuText : MonoBehaviour
{
    private Text text;
    private void OnEnable()
    {
        text = text ?? GetComponent<Text>();
        text.text = "  --Paused--\n";
        text.text += "Turn: " + GameController.Current.Turn + "\n";
        text.text += "Objective:\n" + GameController.Current.GetPauseObjectiveText() + "\n";
        text.text += "Left to move:" + GameController.Current.LeftToMove();
    }
}
