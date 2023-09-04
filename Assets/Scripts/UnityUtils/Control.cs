using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SavedData;

public static class Control
{
    private const KeyCode HARDCODED_START_ALTERNATIVE = KeyCode.Escape;
    public enum CB { A, B, Select, Start }
    public enum CM { Keyboard, Controller }
    public enum Axis { X, Y }

    public static CM ControlMode = CM.Keyboard;
    public static float DeadZone = 0.3f;
    private static bool? _isStartAlternativeBound = null;
    private static bool IsStartAlternativeBound
    {
        get
        {
            if (_isStartAlternativeBound == null)
            {
                _isStartAlternativeBound = false;
                for (CB i = 0; i < CB.Start; i++)
                {
                    if (GetKeyCode(i.ToString()) == HARDCODED_START_ALTERNATIVE)
                    {
                        _isStartAlternativeBound = true;
                    }
                }
                for (Axis i = 0; i < Axis.Y; i++)
                {
                    if (GetKeyCode(i.ToString() + "+") == HARDCODED_START_ALTERNATIVE || GetKeyCode(i.ToString() + "-") == HARDCODED_START_ALTERNATIVE)
                    {
                        _isStartAlternativeBound = true;
                    }
                }
            }
            return _isStartAlternativeBound ?? throw Bugger.FMError("Failed to check whether Escape is bound!");
        }
    }

    public static bool GetButton(CB button)
    {
        if (button == CB.Start && !IsStartAlternativeBound && Input.GetKey(HARDCODED_START_ALTERNATIVE))
        {
            return true;
        }
        return Input.GetKey(GetKeyCode(button.ToString()));
    }

    public static bool GetButtonUp(CB button)
    {
        if (button == CB.Start && !IsStartAlternativeBound && Input.GetKeyUp(HARDCODED_START_ALTERNATIVE))
        {
            return true;
        }
        return Input.GetKeyUp(GetKeyCode(button.ToString()));
    }

    public static bool GetButtonDown(CB button)
    {
        if (button == CB.Start && !IsStartAlternativeBound && Input.GetKeyDown(HARDCODED_START_ALTERNATIVE))
        {
            return true;
        }
        return Input.GetKeyDown(GetKeyCode(button.ToString()));
    }

    public static float GetAxis(Axis axis)
    {
        if (ControlMode == CM.Controller)
        {
            float input = Input.GetAxis(axis == Axis.X ? "Horizontal" : "Vertical");
            if (Mathf.Abs(input) > DeadZone)
            {
                return input;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return Input.GetKey(GetKeyCode(axis + "+")) ? 1 : (Input.GetKey(GetKeyCode(axis + "-")) ? -1 : 0);
        }
    }

    public static int GetAxisInt(Axis axis)
    {
        if (ControlMode == CM.Controller)
        {
            float input = Input.GetAxis(axis == Axis.X ? "Horizontal" : "Vertical");
            if (Mathf.Abs(input) > DeadZone)
            {
                return (int)Mathf.Sign(input);
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return Input.GetKey(GetKeyCode(axis + "+")) ? 1 : (Input.GetKey(GetKeyCode(axis + "-")) ? -1 : 0);
        }
    }

    public static void SetButton(CB button, KeyCode value)
    {
        Save(button + SaveNameModifier(), (int)value, SaveMode.Global);
    }

    public static void SetAxis(Axis axis, KeyCode positiveValue, KeyCode negativeValue)
    {
        SetAxisPositive(axis, positiveValue);
        SetAxisNegative(axis, negativeValue);
    }

    public static void SetAxisPositive(Axis axis, KeyCode positiveValue)
    {
        Save(axis + "+" + SaveNameModifier(), (int)positiveValue, SaveMode.Global);
    }

    public static void SetAxisNegative(Axis axis, KeyCode negativeValue)
    {
        Save(axis + "-" + SaveNameModifier(), (int)negativeValue, SaveMode.Global);
    }

    public static void SetKey(string keySaveName, KeyCode value)
    {
        Save(keySaveName + SaveNameModifier(), (int)value, SaveMode.Global);
    }

    public static string DisplayButtonName(string keySaveName)
    {
        return GetKeyCode(keySaveName).ToString();
    }

    public static string DisplayShortButtonName(string keySaveName)
    {
        return DisplayButtonName(keySaveName).Replace("Arrow", "").Replace("Keypad", "Num").Replace("Alpha", "").Replace("Return", "Enter");
    }

    public static string DisplayShortButtonName(CB button)
    {
        return DisplayShortButtonName(button.ToString());
    }

    public static KeyCode GetKeyCode(string keySaveName)
    {
        return (KeyCode)Load(keySaveName + SaveNameModifier(), 0, SaveMode.Global);
    }

    private static string SaveNameModifier()
    {
        return "ButtonC" + (ControlMode == CM.Controller ? "T" : "F");
    }
}
