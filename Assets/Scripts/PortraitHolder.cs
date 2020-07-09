using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitHolder : MonoBehaviour
{
    public Image Background;
    public Image Foreground;
    public int BackgroundPalleteID = 3;
    private PalettedSprite foregroundPalette;
    public Portrait Portrait
    {
        set
        {
            Background.sprite = value.Background;
            PaletteController.Current.BackgroundPalettes[BackgroundPalleteID] = value.BackgroundColor;
            Foreground.sprite = value.Foreground;
            foregroundPalette.Palette = value.ForegroundColorID;
        }
    }
    private void Awake()
    {
        foregroundPalette = Foreground.GetComponent<PalettedSprite>();
    }
}
