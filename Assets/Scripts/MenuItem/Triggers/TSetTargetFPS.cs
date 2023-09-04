using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSetTargetFPS : Trigger
{
    public List<int> Options;
    private int currentOption;
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
        currentOption = Mathf.Max(0, Options.FindIndex(a => a == Application.targetFrameRate));
        UpdateText();
    }

    public override void Activate()
    {
        camera.UpdateTargetFPS(Options[currentOption = (currentOption + 1) % Options.Count]);
        UpdateText();
    }

    private void UpdateText()
    {
        text.text = Options[currentOption].ToString();
    }
}
