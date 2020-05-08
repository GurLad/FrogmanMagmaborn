using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalettedSprite : MonoBehaviour
{
    public bool Background;
    [Range(0,3)]
    public int Palette;
    private SpriteRenderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        (renderer = GetComponent<SpriteRenderer>()).material = Resources.Load<Material>("Palette");
        for (int i = 0; i < 4; i++)
        {
            renderer.material.SetColor("_Color" + (i + 1) + "out", Background ? PaletteController.Current.BackgroundPalettes[Palette][i] : PaletteController.Current.SpritePalettes[Palette][i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
