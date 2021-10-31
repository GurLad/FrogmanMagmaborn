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
    public Text Upper;
    public Text Lower;
    public PalettedSprite Logo1;
    public PalettedSprite Logo2;
    public Text LogoText;
    public OpeningCutscene OpeningObject;
    public MenuController MenuObject;
    private State state;
    private int currentPart;
    private int lastCheckedCurrent;
    private PaletteTransition transition;
    private bool currentUpper;
    private float count;
    private Text targetText
    {
        get
        {
            return currentUpper ? Upper : Lower;
        }
    }

    public void Begin()
    {
        currentUpper = true;
        targetText.text = Credits[0];
        Logo1.gameObject.SetActive(false);
        Logo2.gameObject.SetActive(false);
        LogoText.text = Lower.text = "";
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
                        targetText.color = PaletteController.Current.BackgroundPalettes[2][1];
                        lastCheckedCurrent = transition.Current;
                    }
                    break;
                case State.TextHold:
                    count += Time.deltaTime;
                    if (count >= HoldTime)
                    {
                        count -= HoldTime;
                        currentPart++;
                        ShowCurrentCredit();
                        state = State.TextHiding;
                        currentUpper = !currentUpper;
                    }
                    break;
                case State.TextHiding:
                    if (transition == null)
                    {
                        currentPart++;
                        if (currentPart >= Credits.Count * 2)
                        {
                            currentPart = 0;
                            Upper.text = Lower.text = "";
                            ShowLogoImage();
                            state = State.ImageShowing;
                        }
                        else
                        {
                            ShowCurrentCredit();
                            state = State.TextShowing;
                        }
                    }
                    else if (lastCheckedCurrent != transition.Current)
                    {
                        if (currentPart + 1 < Credits.Count * 2)
                        {
                            targetText.color = PaletteController.Current.BackgroundPalettes[2][1];
                        }
                        else
                        {
                            Upper.color = PaletteController.Current.BackgroundPalettes[2][1];
                            Lower.color = PaletteController.Current.BackgroundPalettes[2][1];
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
                        LogoText.color = PaletteController.Current.BackgroundPalettes[2][1];
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
        transition = PaletteController.Current.TransitionTo(true, 2, currentPart % 2 == 0 ? CreditsColor : new Palette(), Speed, currentPart % 2 != 0, currentPart % 2 != 0);
        targetText.text = Credits[currentPart / 2];
        targetText.color = currentPart % 2 == 0 ? Color.black : Color.white;
        lastCheckedCurrent = transition.Current;
    }

    private void ShowLogoImage()
    {
        transition = PaletteController.Current.TransitionTo(true, 0, Logo1Palette, Speed);
        transition.AddPalettedSprite(Logo1);
        Logo1.gameObject.SetActive(true);
        transition = PaletteController.Current.TransitionTo(true, 1, Logo2Palette, Speed);
        transition.AddPalettedSprite(Logo2);
        Logo2.gameObject.SetActive(true);
        transition = PaletteController.Current.TransitionTo(true, 2, CreditsColor, Speed);
        LogoText.text = "Disc-O-Key";
        LogoText.color = Color.black;
        lastCheckedCurrent = transition.Current;
    }

    public override void Activate()
    {
        OpeningObject.Activate();
        OpeningObject.OnFinishFadeOut = () => Begin();
    }
}
