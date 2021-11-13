using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSetCameraMode : Trigger
{
    public CameraController.CameraMode CameraMode;
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
        camera.UpdateMode(camera.Mode ^ CameraMode);
        UpdateText();
    }
    private void UpdateText()
    {
        Text.text = (camera.Mode & CameraMode) != CameraController.CameraMode.Default ? "On" : " Off";
    }
}
