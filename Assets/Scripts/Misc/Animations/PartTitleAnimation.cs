using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartTitleAnimation : MidBattleScreen
{
    [Header("Stats")]
    public float DisplaySpeed;
    public float FullDelay;
    public Palette Palette;
    [Header("Objects")]
    public List<PalettedText> TitleParts;
    private float count;
    private int currentPart;
    private int displayPart;
    private PaletteController.PaletteControllerState previousState;

    public void Begin(List<string> titles)
    {
        if (titles.Count > TitleParts.Count)
        {
            throw Bugger.Error("More titles than the animation can support! Sent " + titles.Count + " > " + TitleParts.Count);
        }
        for (int i = 0; i < titles.Count; i++)
        {
            TitleParts[i].Text.text = titles[i];
            TitleParts[i].Palette = 2;
        }
        count = currentPart = displayPart = 0;
        TitleParts[0].Palette = 0;
        previousState = PaletteController.Current.SaveState();
        PaletteController.Current.SpritePalettes[0][2] =
            PaletteController.Current.SpritePalettes[0][1] =
            PaletteController.Current.SpritePalettes[1][2] =
            PaletteController.Current.SpritePalettes[2][2] =
            PaletteController.Current.SpritePalettes[2][1] = Palette[0];
        PaletteController.Current.SpritePalettes[1][1] = Palette[3];
    }

    private void Update()
    {
        count += Time.deltaTime;
        if (currentPart == TitleParts.Count && FullDelay > 0)
        {
            if (count >= FullDelay)
            {
                count -= FullDelay;
                FullDelay = -1;
            }
            else
            {
                return;
            }
        }
        if (count >= 1 / DisplaySpeed)
        {
            count -= 1 / DisplaySpeed;
            displayPart += 1;
            displayPart %= 4;
            if (displayPart == 0)
            {
                if (currentPart < TitleParts.Count)
                {
                    TitleParts[currentPart].Palette = 1;
                }
                currentPart++;
                if (currentPart < TitleParts.Count)
                {
                    TitleParts[currentPart].Palette = 0;
                }
            }
            if (currentPart < TitleParts.Count)
            {
                PaletteController.Current.SpritePalettes[0][1] = Palette[displayPart];
            }
            else if (currentPart == TitleParts.Count)
            {
                PaletteController.Current.SpritePalettes[1][1] = Palette[3 - displayPart];
            }
            else
            {
                Quit(true, () => ConversationPlayer.Current.Resume(), previousState);
            }
        }
    }
}
