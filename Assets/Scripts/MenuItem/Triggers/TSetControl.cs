using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TSetControl : Trigger
{
    public enum Keys { Left, Right, Up, Down, A, B, Select, Start }
    public Keys Key;
    public MenuController Source;
    private Text text;
    private PalettedText palettedText;
    private bool waitingForInput = false;
    private bool waitAFrame = false;
    private System.Action postWaitAction;
    private readonly KeyCode[] keyCodes = System.Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();
    private static List<KeyCode> taken { get; } = new List<KeyCode>(new KeyCode[8]); // Yes, yes, global statics are bad, but I'm tired and lazy

    private void Start()
    {
        text = GetComponent<Text>();
        palettedText = GetComponent<PalettedText>();
        UpdateText(true);
    }

    private void Update()
    {
        if (waitAFrame) // Wait a frame
        {
            waitAFrame = false;
            return;
        }
        if (postWaitAction != null)
        {
            postWaitAction();
            postWaitAction = null;
        }
        if (waitingForInput)
        {
            KeyCode keyCode = GetKey();
            if (keyCode != KeyCode.None && !taken.Contains(keyCode))
            {
                Control.SetKey(GetKeySaveName(), keyCode);
                SavedData.SaveAll(SaveMode.Global);                
                waitingForInput = false;
                waitAFrame = true;
                postWaitAction = () => Source.enabled = true;
                UpdateText();
            }
        }
    }

    private KeyCode GetKey()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    return keyCode;
                }
            }
        }
        return KeyCode.None;
    }

    public override void Activate()
    {
        waitingForInput = true;
        Source.enabled = false;
        waitAFrame = true;
        UpdateText();
    }

    private void UpdateText(bool firstTime = false)
    {
        text.text = waitingForInput ? "Press" : Control.DisplayShortButtonName(GetKeySaveName());
        taken[(int)Key] = waitingForInput ? KeyCode.None : Control.GetKeyCode(GetKeySaveName());
        if (!firstTime)
        {
            palettedText.Palette = waitingForInput ? 2 : 0;
        }
    }

    private string GetKeySaveName()
    {
        switch (Key)
        {
            case Keys.Left:
                return "X-";
            case Keys.Right:
                return "X+";
            case Keys.Up:
                return "Y+";
            case Keys.Down:
                return "Y-";
            default:
                return Key.ToString();
        }
    }
}
