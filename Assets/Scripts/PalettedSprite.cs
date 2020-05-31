using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PalettedSprite : MonoBehaviour
{
    public bool Background;
    [Range(0,3)]
    public int Palette;
    private bool ui;
    private SpriteRenderer renderer;
    private Image image;
    // Start is called before the first frame update
    void Start()
    {
        ui = (renderer = GetComponent<SpriteRenderer>()) == null;
        if (!ui)
        {
            renderer.material = Resources.Load<Material>("Palette");
            for (int i = 0; i < 4; i++)
            {
                renderer.material.SetColor("_Color" + (i + 1) + "out", Background ? PaletteController.Current.BackgroundPalettes[Palette][i] : PaletteController.Current.SpritePalettes[Palette][i]);
            }
        }
        else
        {
            (image = GetComponent<Image>()).material = Resources.Load<Material>("Palette");
            for (int i = 0; i < 4; i++)
            {
                image.material.SetColor("_Color" + (i + 1) + "out", Background ? PaletteController.Current.BackgroundPalettes[Palette][i] : PaletteController.Current.SpritePalettes[Palette][i]);
            }
        }
    }
}
