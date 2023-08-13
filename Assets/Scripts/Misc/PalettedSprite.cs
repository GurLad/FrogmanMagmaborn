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
    public Sprite Sprite
    {
        get
        {
            if (!initialized)
            {
                throw Bugger.FMError("Uninitialized PalettedSprite.");
            }
            return !ui ? renderer.sprite : image.sprite;
        }
        set
        {
            if (!initialized)
            {
                throw Bugger.FMError("Uninitialized PalettedSprite.");
            }
            if (!ui)
            {
                renderer.sprite = value;
            }
            else
            {
                image.sprite = value;
            }
        }
    }
    private bool ui;
    private new SpriteRenderer renderer;
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
            renderer.sharedMaterial = PaletteController.Current.GetMaterial(Background, palette);
        }
        else
        {
            (image = GetComponent<Image>()).material = PaletteController.Current.GetMaterial(Background, palette);
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
            throw Bugger.FMError("Uninitialized PalettedSprite.");
        }
        if (!ui)
        {
            for (int i = 0; i < 4; i++)
            {
                renderer.sharedMaterial = PaletteController.Current.GetMaterial(Background, palette);
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                image.material = PaletteController.Current.GetMaterial(Background, palette);
            }
        }
    }

    public void ForceSilentSetPalette(int index) // Very bad workaround for FFImporter
    {
        palette = index;
    }
}
