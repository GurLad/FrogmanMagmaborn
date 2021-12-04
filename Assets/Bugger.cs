using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bugger : MonoBehaviour // Custom name for alternate Debug
{
    public Sprite[] StatusTypes;
    [Header("Objects")]
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
        gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public static void Info(string text, bool show = false)
    {
        if (current == null)
        {
            Debug.Log("No bugger! Anyway, " + text);
            return;
        }
        current.ShowText("INFO: " + text.ToColoredString(3), 0, show);
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
        current.ShowText("WARNING: " + text.ToColoredString(2), 0, show);
#if UNITY_EDITOR
        Debug.LogWarning(text);
#endif
    }

    public static System.Exception Error(string text, bool show = true)
    {
        current.ShowText("ERROR: " + text.ToColoredString(1), 1, show);
        return new System.Exception(text);
    }

    public static System.Exception Crash(string text, bool show = true)
    {
        current.ShowText("FATAL ERROR: " + text.ToColoredString(1, 2), 2, show);
        return new System.Exception(text);
    }

    private void ShowText(string text, int statusType, bool show = true)
    {
        Text.text = text + "\n\n" + Text.text;
        Status.sprite = StatusTypes[statusType];
        if (show)
        {
            gameObject.SetActive(true);
        }
    }
}
