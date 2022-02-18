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
    private Portrait _portrait = null;
    public Portrait Portrait
    {
        set
        {
            PaletteController.Current.BackgroundPalettes[BackgroundPalleteID].CopyFrom(value.BackgroundColor);
            Background.sprite = value.Background;
            backgroundPalette.UpdatePalette();
            Foreground.sprite = value.Foreground;
            foregroundPalette.Palette = value.ForegroundColorID;
            _portrait = value;
        }
        get
        {
            return _portrait;
        }
    }
    public void Awake()
    {
        GetComponent<PalettedSprite>().Awake();
        foregroundPalette = Foreground.GetComponent<PalettedSprite>();
        foregroundPalette.Awake();
        backgroundPalette = Background.GetComponent<PalettedSprite>();
        backgroundPalette.Awake();
        backgroundPalette.Palette = BackgroundPalleteID;
    }
}
