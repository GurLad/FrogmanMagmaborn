using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgamePaletteCheater : MonoBehaviour
{
    public bool Background = true;
    [Range(0, 3)]
    public int ReferencePalette = 2;
    [Range(0, 3)]
    public int ReferenceColour = 0;
    public List<Palette> TrueColours;
    public List<Material> Materials;

    private void Reset()
    {
        TrueColours = new List<Palette>();
        TrueColours.Add(new Palette());
    }

    private void LateUpdate()
    {
        int palette = Background ? PaletteController.Current.BackgroundPalettes[ReferencePalette][ReferenceColour] : PaletteController.Current.SpritePalettes[ReferencePalette][ReferenceColour];
        int jump = CompletePalette.BrightnessJump * ((palette + 1) / CompletePalette.BrightnessJump);
        for (int i = 0; i < Materials.Count; i++)
        {
            Materials[i].color = CompletePalette.Colors[Mathf.Min(CompletePalette.BlackColor, TrueColours[i / 4][i % 4] + jump)];
        }
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        for (int i = 0; i < Materials.Count; i++)
        {
            Materials[i].color = CompletePalette.Colors[TrueColours[i / 4][i % 4]];
        }
    }
#endif
}
