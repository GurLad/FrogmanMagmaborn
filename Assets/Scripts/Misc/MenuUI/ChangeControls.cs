using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChangeControls : MonoBehaviour
{
    private enum Keys { Left, Right, Up, Down, A, B, Select, Start }
    public List<string> Descriptions;
    public Text Text;
    public MenuController Source;
    private int currentKey;
    private readonly KeyCode[] keyCodes = System.Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();
    private void OnEnable()
    {
        currentKey = 0;
        UpdateDisplay();
    }
    private void Update()
    {
        KeyCode keyCode = GetKey();
        if (keyCode != KeyCode.None)
        {
            Control.SetKey(GetKeySaveName(), keyCode);
            currentKey++;
            if (currentKey >= Descriptions.Count)
            {
                SavedData.SaveAll(SaveMode.Global);
                gameObject.SetActive(false);
                Source.Begin();
            }
            else
            {
                UpdateDisplay();
            }
        }
    }
    private KeyCode GetKey()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKey(keyCode))
                {
                    return keyCode;
                }
            }
        }
        return KeyCode.None;
    }
    private void UpdateDisplay()
    {
        //ErrorController.Info("Key: " + ((Keys)currentKey) + ", save name: " + GetKeySaveName() + ", display button: " + Control.DisplayButtonName(GetKeySaveName()));
        Text.text = "Press " + ((Keys)currentKey).ToString() + "\n" + Descriptions[currentKey] + "\nCurrent: " + Control.DisplayShortButtonName(GetKeySaveName());
    }
    private string GetKeySaveName()
    {
        switch ((Keys)currentKey)
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
                return ((Keys)currentKey).ToString();
        }
    }
}
