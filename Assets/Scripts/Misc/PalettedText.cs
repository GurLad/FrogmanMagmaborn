using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PalettedText : MonoBehaviour
{
    public Text Text;
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
    [Range(0, 3)]
    private int palette = 3;
    private bool initialized = false;

    public void Awake()
    {
        if (initialized)
        {
            return;
        }
        (Text = GetComponent<Text>()).material = PaletteController.Current.GetTextMaterial();
        initialized = true;
        Palette = Palette;
    }

    private void UpdatePalette()
    {
        if (!initialized)
        {
            throw Bugger.Error("Uninitialized PalettedSprite - this is a Frogman Magmaborn error. Please report to the devs.");
        }
        Color temp;
        ColorUtility.TryParseHtmlString("#" + Palette + "00000", out temp);
        Text.color = temp;
    }
}