using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteController : MonoBehaviour
{
    public static PaletteController Current;
    public Palette[] BackgroundPalettes = new Palette[4];
    public Palette[] SpritePalettes = new Palette[4];

    private void Awake()
    {
        Current = this;
    }
}

[System.Serializable]
public class Palette
{
    public Color[] colors = new Color[4];
    public Color this[int i]
    {
        get
        {
            return colors[i];
        }
    }
}