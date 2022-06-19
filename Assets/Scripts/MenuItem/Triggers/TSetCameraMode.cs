using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSetCameraMode : Trigger
{
    public CameraController.CameraMode CameraMode;
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
        camera.UpdateMode(camera.Mode ^ CameraMode);
        UpdateText();
    }
    private void UpdateText()
    {
        text.text = (camera.Mode & CameraMode) != CameraController.CameraMode.Default ? "On" : " Off";
    }
}
