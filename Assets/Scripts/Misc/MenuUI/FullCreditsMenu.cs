using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullCreditsMenu : Trigger
{
    private enum State { TextShowing, TextHold, TextHiding, ImageShowing, ImageHold }
    [Header("Data")]
    [Multiline]
    public List<string> Credits;
    public float Speed;
    public float HoldTime;
    public Palette CreditsColor;
    public Palette Logo1Palette;
    public Palette Logo2Palette;
    [Header("Objects")]
    public PalettedText Upper;
    public PalettedText Lower;
    public PalettedSprite Logo1;
    public PalettedSprite Logo2;
    public PalettedText LogoText;
    public OpeningCutscene OpeningObject;
    public MenuController MenuObject;
    private State state;
    private int currentPart;
    private int lastCheckedCurrent;
    private PaletteTransition transition;
    private bool currentUpper;
    private float count;
    private Palette creditsReverse;
    private PalettedText targetText
    {
        get
        {
            return currentUpper ? Upper : Lower;
        }
    }
    private PalettedText notTargetText
    {
        get
        {
            return !currentUpper ? Upper : Lower;
        }
    }

    public void Begin()
    {
        creditsReverse = new Palette();
        for (int i = 1; i < 4; i++)
        {
            creditsReverse[i] = CreditsColor[4 - i];
        }
        currentUpper = true;
        PaletteController.Current.SpritePalettes[3][1] = CreditsColor[1];
        Upper.Awake();
        Lower.Awake();
        LogoText.Awake();
        targetText.Text.text = Credits[0];
        targetText.Palette = 2;
        Logo1.gameObject.SetActive(false);
        Logo2.gameObject.SetActive(false);
        LogoText.Text.text = Lower.Text.text = "";
        count = 0;
        currentPart = 0;
        lastCheckedCurrent = 0;
        transition = null;
        state = State.TextShowing;
        ShowCurrentCredit();
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.Start))
        {
            OpeningObject.Restore();
            MenuObject.Begin();
            gameObject.SetActive(false);
            state = State.TextShowing;
            currentPart = 0;
            lastCheckedCurrent = 0;
            count = 0;
            Destroy(transition);
        }
        else
        {
            switch (state)
            {
                case State.TextShowing:
                    if (transition == null)
                    {
                        count = 0;
                        state = State.TextHold;
                    }
                    else if (lastCheckedCurrent != transition.Current)
                    {
                        //targetText.color = PaletteController.Current.BackgroundPalettes[2][1];
                        lastCheckedCurrent = transition.Current;
                    }
                    break;
                case State.TextHold:
                    count += Time.deltaTime;
                    if (count >= HoldTime)
                    {
                        count -= HoldTime;
                        currentPart++;
                        currentUpper = !currentUpper;
                        if (targetText.Text.text != "")
                        {
                            ShowCurrentCredit();
                            state = State.TextHiding;
                        }
                        else
                        {
                            currentPart++;
                            targetText.Text.text = Credits[currentPart / 2];
                            ShowCurrentCredit();
                            state = State.TextShowing;
                        }
                    }
                    break;
                case State.TextHiding:
                    if (transition == null)
                    {
                        currentPart++;
                        if (currentPart >= Credits.Count * 2)
                        {
                            currentPart = 0;
                            Upper.Text.text = Lower.Text.text = "";
                            ShowLogoImage();
                            state = State.ImageShowing;
                        }
                        else
                        {
                            targetText.Text.text = Credits[currentPart / 2];
                            ShowCurrentCredit();
                            state = State.TextShowing;
                        }
                    }
                    else if (lastCheckedCurrent != transition.Current)
                    {
                        if (currentPart + 1 < Credits.Count * 2)
                        {
                            //targetText.color = PaletteController.Current.BackgroundPalettes[2][1];
                        }
                        else
                        {
                            //Upper.color = PaletteController.Current.BackgroundPalettes[2][1];
                            //Lower.color = PaletteController.Current.BackgroundPalettes[2][1];
                        }
                        lastCheckedCurrent = transition.Current;
                    }
                    break;
                case State.ImageShowing:
                    if (transition == null)
                    {
                        count = 0;
                        state = State.ImageHold;
                    }
                    else if (lastCheckedCurrent != transition.Current)
                    {
                        lastCheckedCurrent = transition.Current;
                    }
                    break;
                case State.ImageHold:
                    break;
                default:
                    break;
            }
        }
    }

    private void ShowCurrentCredit()
    {
        transition = PaletteController.Current.PaletteTransitionTo(false, 2, currentPart % 2 == 0 ? CreditsColor : creditsReverse, Speed, currentPart % 2 == 1);
        targetText.Palette = 2;
        notTargetText.Palette = currentPart + 1 < Credits.Count * 2 ? 3 : 2;
        lastCheckedCurrent = transition.Current;
    }

    private void ShowLogoImage()
    {
        transition = PaletteController.Current.PaletteTransitionTo(true, 0, Logo1Palette, Speed);
        Logo1.gameObject.SetActive(true);
        transition = PaletteController.Current.PaletteTransitionTo(true, 1, Logo2Palette, Speed);
        Logo2.gameObject.SetActive(true);
        transition = PaletteController.Current.PaletteTransitionTo(false, 3, CreditsColor, Speed);
        LogoText.Text.text = "Disc-O-Key";
        LogoText.Palette = 3;
        lastCheckedCurrent = transition.Current;
    }

    public override void Activate()
    {
        OpeningObject.Activate();
        OpeningObject.OnFinishFadeOut = () => Begin();
    }
}
