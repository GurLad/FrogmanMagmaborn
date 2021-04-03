using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    public List<GameObject> Indicators;
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
    private List<Trigger> triggers;
    private void Awake()
    {
        text = GetComponent<Text>();
        triggers = new List<Trigger>(GetComponents<Trigger>());
        Unselect();
    }
    public virtual void Select()
    {
        Indicators.ForEach(a => a.SetActive(true));
        text.color = PaletteController.Current.SpritePalettes[0][1];
    }
    public virtual void Unselect()
    {
        Indicators.ForEach(a => a.SetActive(false));
        text.color = PaletteController.Current.SpritePalettes[3][1];
    }
    public virtual void Activate()
    {
        triggers.ForEach(a => a.Activate());
    }
}
