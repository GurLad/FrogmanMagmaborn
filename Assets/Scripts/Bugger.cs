using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bugger : MonoBehaviour // Custom name for alternate Debug
{
    public Sprite[] StatusTypes;
    [Header("Objects")]
    public GameObject BuggerObject;
    public Text Text;
    public Image Status;
    private static Bugger current;

    private void Awake()
    {
        if (current != null)
        {
            Destroy(gameObject);
            return;
        }
        current = this;
        BuggerObject.SetActive(false);
    }

    private void Update()
    {
        if (GameCalculations.Debug && Input.GetKeyUp(KeyCode.F10))
        {
            BuggerObject.SetActive(!BuggerObject.activeSelf);
        }
    }

    public void Hide()
    {
        BuggerObject.SetActive(false);
    }

    public static void Info(string text, bool show = false)
    {
        if (current == null)
        {
            Debug.Log("No bugger! Anyway, " + text);
            return;
        }
        current.ShowText(ColorString("INFO: " + text, "FFFFFF"), 0, show);
#if UNITY_EDITOR
        Debug.Log(text);
#endif
    }

    public static void Warning(string text, bool show = false)
    {
        if (current == null)
        {
            Debug.LogWarning("No bugger! Anyway, " + text);
            return;
        }
        current.ShowText(ColorString("WARNING: " + text, "FFFF00"), 0, show);
#if UNITY_EDITOR
        Debug.LogWarning(text);
#endif
    }

    public static System.Exception Error(string text, bool show = true)
    {
        current.ShowText(ColorString("ERROR: " + text, "FF5000"), 1, show);
        return new System.Exception(text);
    }

    public static System.Exception Crash(string text, bool show = true)
    {
        current.ShowText(ColorString("FATAL ERROR: " + text, "FF0000"), 2, show);
        return new System.Exception(text);
    }

    public static System.Exception FMError(string text, bool show = true)
    {
        current.ShowText(ColorString("FROGMAN MAGMABORN ERROR: " + text + " Please send a bug report.", "FF00FF"), 1, show);
        return new System.Exception(text);
    }

    private static string ColorString(string str, string color)
    {
        return "<color=#" + color + ">" + str + "</color>";
    }

    private void ShowText(string text, int statusType, bool show = true)
    {
        Text.text = text + "\n\n" + Text.text;
        Status.sprite = StatusTypes[statusType];
        if (show && GameCalculations.Debug)
        {
            BuggerObject.SetActive(true);
        }
    }
}
