using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    public bool UseCancelSFX;
    public List<GameObject> Indicators;
    [HideInInspector]
    public RectTransform TheRectTransform;
    public string Text
    {
        get
        {
            return text.text;
        }
        set
        {
            (text ??= GetComponent<Text>()).text = value;
        }
    }
    private Text text;
    private PalettedText palettedText;
    private List<Trigger> triggers;

    public void Awake()
    {
        TheRectTransform = GetComponent<RectTransform>();
        text = GetComponent<Text>();
        palettedText = GetComponent<PalettedText>();
        if (palettedText != null) // For StatusMenuItem...
        {
            palettedText.Awake();
        }
        triggers = new List<Trigger>(GetComponents<Trigger>());
        Unselect();
    }

    public virtual void Select()
    {
        Indicators.ForEach(a => a.SetActive(true));
        palettedText.Palette = 0;
    }

    public virtual void Unselect()
    {
        Indicators.ForEach(a => a.SetActive(false));
        palettedText.Palette = 3;
    }

    public virtual void Activate()
    {
        triggers.ForEach(a => a.Activate());
        SystemSFXController.Play(UseCancelSFX ? SystemSFXController.Type.MenuCancel : SystemSFXController.Type.MenuSelect);
    }

    public virtual void OnMenuDone()
    {
        // Do nothing
    }
}
