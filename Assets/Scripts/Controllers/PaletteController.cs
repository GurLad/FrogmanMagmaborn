using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteController : MonoBehaviour
{
    public static PaletteController Current
    {
        get
        {
            if (current == null)
            {
                current = FindObjectOfType<PaletteController>();
            }
            return current;
        }
        private set => current = value;
    }
    public Palette[] BackgroundPalettes = new Palette[4];
    public Palette[] SpritePalettes = new Palette[4];
    private static PaletteController current;

    private void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            BackgroundPalettes[i].Colors[0] = Color.black;
            BackgroundPalettes[i].Colors[1] = Color.white;
            BackgroundPalettes[i].Colors[2] = Color.gray;
            BackgroundPalettes[i].Colors[3] = Color.black;
            SpritePalettes[i].Colors[0] = Color.black;
            SpritePalettes[i].Colors[1] = Color.white;
            SpritePalettes[i].Colors[2] = Color.gray;
            SpritePalettes[i].Colors[3] = new Color(1,0,1,0);
        }
    }

    private void Awake()
    {
        current = this;
    }
}

[System.Serializable]
public class Palette
{
    public Color[] Colors = new Color[4];
    public Color this[int i]
    {
        get
        {
            return Colors[i];
        }
    }
    public Palette()
    {
        for (int i = 0; i < 4; i++)
        {
            Colors[i] = Color.black;
        }
    }
}