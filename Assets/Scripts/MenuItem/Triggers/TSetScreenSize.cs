using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSetScreenSize : Trigger
{
    private Text Text;
    private CameraController camera
    {
        get
        {
            return CameraController.Current;
        }
    }
    private void Start()
    {
        Text = GetComponent<Text>();
        UpdateText();
    }
    public override void Activate()
    {
        camera.ChangeSize(1);
        UpdateText();
    }
    private void UpdateText()
    {
        Text.text = "X" + (camera.CurrentMultiplier + 1);
    }
}
