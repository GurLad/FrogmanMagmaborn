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
    public List<Text> TitleParts;
    private float count;
    private int currentPart;
    private int displayPart;

    public void Begin(List<string> titles)
    {
        if (titles.Count > TitleParts.Count)
        {
            throw Bugger.Error("More titles than the animation can support! Sent " + titles.Count + " > " + TitleParts.Count);
        }
        for (int i = 0; i < titles.Count; i++)
        {
            TitleParts[i].text = titles[i];
        }
        count = currentPart = displayPart = 0;
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
                currentPart++;
            }
            if (currentPart < TitleParts.Count)
            {
                TitleParts[currentPart].color = Palette[displayPart];
            }
            else if (currentPart == TitleParts.Count)
            {
                TitleParts.ForEach(a => a.color = Palette[3 - displayPart]);
            }
            else
            {
                Quit(true, () => ConversationPlayer.Current.Resume());
            }
        }
    }
}
