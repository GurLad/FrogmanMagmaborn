using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpeningCutscene : Trigger
{
    [Header("Data")]
    public List<string> Credits;
    public Palette CreditsColor;
    public List<Image> ImageParts;
    public List<Palette> ImagePalettes = new List<Palette>(new Palette[] { new Palette() });
    public float Speed;
    [Header("Objects")]
    public Text CreditsObject;
    public GameObject PressStart;
    public GameObject IntroObject;
    private bool fadeOut;
    private bool finishedCredits;
    private int currentPart;
    private int lastCheckedCurrent;
    private PaletteTransition transition;
    private Palette creditsReverse;

    public override void Activate()
    {
        enabled = fadeOut = true;
        transition = null;
        for (int i = 0; i < ImageParts.Count; i++)
        {
            transition = PaletteController.Current.TransitionTo(true, i, new Palette(), Speed, true, true);
        }
        lastCheckedCurrent = transition.Current;
    }

    private void Start()
    {
        transition = PaletteController.Current.TransitionTo(true, 0, CreditsColor, Speed);
        CreditsObject.text = Credits[0];
        creditsReverse = new Palette();
        for (int i = 1; i < 4; i++)
        {
            creditsReverse.Colors[i] = CreditsColor[4 - i];
        }
    }
    private void Update()
    {
        if (fadeOut)
        {
            if (transition == null)
            {
                gameObject.SetActive(false);
                IntroObject.SetActive(true);
                Destroy(this);
            }
            else if (lastCheckedCurrent != transition.Current)
            {
                for (int i = 0; i < ImageParts.Count; i++)
                {
                    ImageParts[i].GetComponent<PalettedSprite>().UpdatePalette();
                }
                lastCheckedCurrent = transition.Current;
            }
        }
        else
        {
            if (Control.GetButtonDown(Control.CB.Start))
            {
                if (transition != null)
                {
                    Destroy(transition);
                }
                for (int i = 0; i < ImageParts.Count; i++)
                {
                    PaletteController.Current.BackgroundPalettes[i] = ImagePalettes[i];
                    ImageParts[i].gameObject.SetActive(true);
                    ImageParts[i].GetComponent<PalettedSprite>().UpdatePalette();
                }
                PressStart.SetActive(true);
                enabled = false;
                if (CreditsObject != null)
                {
                    Destroy(CreditsObject.gameObject);
                }
                return;
            }
            if (transition == null)
            {
                currentPart++;
                if (finishedCredits && currentPart >= ImageParts.Count)
                {
                    PressStart.SetActive(true);
                    enabled = false;
                    return;
                }
                else if (!finishedCredits && currentPart >= Credits.Count * 2)
                {
                    finishedCredits = true;
                    currentPart = 0;
                    Destroy(CreditsObject.gameObject);
                }
                if (finishedCredits)
                {
                    transition = PaletteController.Current.TransitionTo(true, currentPart, ImagePalettes[currentPart], Speed);
                    ImageParts[currentPart].gameObject.SetActive(true);
                }
                else
                {
                    transition = PaletteController.Current.TransitionTo(true, 0, currentPart % 2 == 0 ? CreditsColor : creditsReverse, Speed);
                    CreditsObject.text = Credits[currentPart / 2];
                    CreditsObject.color = currentPart % 2 == 0 ? Color.black : Color.white;
                }
                lastCheckedCurrent = transition.Current;
                return;
            }
            if (lastCheckedCurrent != transition.Current)
            {
                if (finishedCredits)
                {
                    ImageParts[currentPart].GetComponent<PalettedSprite>().UpdatePalette();
                }
                else
                {
                    CreditsObject.color = PaletteController.Current.BackgroundPalettes[0][1];
                }
                lastCheckedCurrent = transition.Current;
            }
        }
    }
}

/*
 * Replicate the NES Fire Emblem opening:
 * 1. Year-company-presents
 * 2. Fade in the logo
 * 3. Show the menu
 */ 
