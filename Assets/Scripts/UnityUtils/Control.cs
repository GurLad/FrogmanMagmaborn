using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SavedData;

public static class Control
{
    public enum CB { A, B }
    public enum CM { Keyboard, Controller }
    public static CM ControlMode = CM.Keyboard;

    public static bool GetButton(CB button)
    {
        return Input.GetKey(GetKeyCode(button.ToString()));
    }

    public static bool GetButtonUp(CB button)
    {
        return Input.GetKeyUp(GetKeyCode(button.ToString()));
    }

    public static bool GetButtonDown(CB button)
    {
        return Input.GetKeyDown(GetKeyCode(button.ToString()));
    }

    public static float GetAxis(SnapAxis axis)
    {
        if (ControlMode == CM.Controller)
        {
            return Input.GetAxis("Horizontal");
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

    public static void SetAxis(SnapAxis axis, KeyCode positiveValue, KeyCode negativeValue)
    {
        SetAxisPositive(axis, positiveValue);
        SetAxisNegative(axis, negativeValue);
    }

    public static void SetAxisPositive(SnapAxis axis, KeyCode positiveValue)
    {
        Save(axis + "+" + SaveNameModifier(), (int)positiveValue, SaveMode.Global);
    }

    public static void SetAxisNegative(SnapAxis axis, KeyCode negativeValue)
    {
        Save(axis + "-" + SaveNameModifier(), (int)negativeValue, SaveMode.Global);
    }

    public static string DisplayButtonName(string keySaveName)
    {
        return GetKeyCode(keySaveName).ToString();
    }

    private static KeyCode GetKeyCode(string keySaveName)
    {
        return (KeyCode)Load(keySaveName + SaveNameModifier(), 0, SaveMode.Global);
    }

    private static string SaveNameModifier()
    {
        return "ButtonC" + (ControlMode == CM.Controller ? "T" : "F");
    }
}
