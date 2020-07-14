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
    private PalettedSprite backgroundPalette;
    public Portrait Portrait
    {
        set
        {
            PaletteController.Current.BackgroundPalettes[BackgroundPalleteID] = value.BackgroundColor;
            Background.sprite = value.Background;
            backgroundPalette.UpdatePalette();
            Foreground.sprite = value.Foreground;
            foregroundPalette.Palette = value.ForegroundColorID;
        }
    }
    private void Awake()
    {
        foregroundPalette = Foreground.GetComponent<PalettedSprite>();
        backgroundPalette = Background.GetComponent<PalettedSprite>();
    }
}
