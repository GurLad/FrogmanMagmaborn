using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PalettedSprite : MonoBehaviour
{
    public bool Background;
    public int Palette
    {
        get
        {
            return palette;
        }
        set
        {
            palette = value;
            UpdatePalette();
        }
    }
    [SerializeField]
    [Range(0,3)]
    private int palette;
    private bool ui;
    private SpriteRenderer renderer;
    private Image image;
    private bool initialized = false;

    public void Awake()
    {
        if (initialized)
        {
            return;
        }
        if (!enabled)
        {
            // This is so weird. Probably related to the "no Start" bug on units created with CreatePlayerUnit?
            enabled = true;
        }
        ui = (renderer = GetComponent<SpriteRenderer>()) == null;
        if (!ui)
        {
            renderer.material = Resources.Load<Material>("Palette");
        }
        else
        {
            (image = GetComponent<Image>()).material = Instantiate(Resources.Load<Material>("Palette"));
        }
        initialized = true;
    }

    private void Start()
    {
        UpdatePalette();
    }

    public void UpdatePalette()
    {
        if (!initialized)
        {
            throw Bugger.Error("Uninitialized PalettedSprite - this is a Frogman Magmaborn error. Please report to the devs.");
        }
        if (!ui)
        {
            for (int i = 0; i < 4; i++)
            {
                renderer.material.SetColor("_Color" + (i + 1) + "out", Background ? PaletteController.Current.BackgroundPalettes[Palette][i] : PaletteController.Current.SpritePalettes[Palette][i]);
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                image.material.SetColor("_Color" + (i + 1) + "out", Background ? PaletteController.Current.BackgroundPalettes[Palette][i] : PaletteController.Current.SpritePalettes[Palette][i]);
            }
        }
    }

    public void ForceSilentSetPalette(int index) // Very bad workaround for FFImporter
    {
        palette = index;
    }
}
