using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortraitController : MonoBehaviour
{
    public static PortraitController Current;
    public List<Portrait> Portraits;
    public Portrait ErrorPortrait;
    private void Reset()
    {
        Portraits.Add(new Portrait());
    }
    private void Awake()
    {
        Current = this;
    }
    public Portrait FindPortrait(string name)
    {
        return Portraits.Find(a => a.Name == name) ?? ErrorPortrait;
    }
}

[System.Serializable]
public class Portrait
{
    public string Name;
    public Sprite Background;
    public Sprite Foreground;
    public Palette BackgroundColor = new Palette();
    [Range(0, 3)]
    public int ForegroundColorID;
    public Portrait()
    {
        BackgroundColor = new Palette();
        for (int i = 0; i < 4; i++)
        {
            BackgroundColor.Colors[i] = Color.black;
        }
    }
}