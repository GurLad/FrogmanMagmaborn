using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSetCameraMode : Trigger
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
        camera.UpdateMode(camera.Mode == CameraController.CameraMode.PixelPerfect ? CameraController.CameraMode.Stretch : CameraController.CameraMode.PixelPerfect);
        UpdateText();
    }
    private void UpdateText()
    {
        Text.text = camera.Mode.ToString();
    }
}
