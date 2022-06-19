using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSetScreenSize : Trigger
{
    private Text text;
    private new CameraController camera
    {
        get
        {
            return CameraController.Current;
        }
    }
    private void Start()
    {
        text = GetComponent<Text>();
        UpdateText();
    }
    public override void Activate()
    {
        camera.ChangeSize(1);
        UpdateText();
    }
    private void UpdateText()
    {
        text.text = "X" + (camera.CurrentMultiplier + 1);
    }
}
