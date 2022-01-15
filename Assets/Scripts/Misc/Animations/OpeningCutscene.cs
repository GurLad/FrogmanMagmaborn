using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpeningCutscene : Trigger
{
    private enum State { TextShowing, TextHold, TextHiding, ImageShowing, ImageHold }
    [Header("Data")]
    public List<string> Credits;
    public Palette CreditsColor;
    public List<Image> ImageParts;
    public List<Palette> ImagePalettes = new List<Palette>(new Palette[] { new Palette() });
    public float Speed;
    public float HoldTime;
    [Header("Objects")]
    public Text CreditsObject;
    public GameObject PressStart;
    [HideInInspector]
    public System.Action OnFinishFadeOut;
    private bool fadeOut;
    private State state;
    private int currentPart;
    private int lastCheckedCurrent;
    private PaletteTransition transition;
    private Palette creditsReverse;
    private float count;

    public override void Activate()
    {
        enabled = fadeOut = true;
        transition = null;
        for (int i = 0; i < ImageParts.Count; i++)
        {
            transition = PaletteController.Current.TransitionTo(true, i, new Palette(), Speed, true, true);
        }
        lastCheckedCurrent = transition.Current;
        count = 0;
        OnFinishFadeOut = () =>
        {
            if (SavedData.Load("HasSuspendData", 0) != 0)
            {
                SceneController.LoadScene("Map");
            }
            else
            {
                ConversationPlayer.Current.Play(ConversationController.Current.SelectConversation());
                Destroy(this);
            }
        };
    }

    private void Start()
    {
        transition = PaletteController.Current.TransitionTo(true, 0, CreditsColor, Speed);
        CreditsObject.text = Credits[0];
        state = State.TextShowing;
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
                count += Time.deltaTime;
                if (count >= HoldTime)
                {
                    gameObject.SetActive(false);
                    OnFinishFadeOut();
                }
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
                    PaletteController.Current.BackgroundPalettes[i] = ImagePalettes[i].Clone();
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
                            CreditsObject.color = PaletteController.Current.BackgroundPalettes[0][1];
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
                        }
                        break;
                    case State.TextHiding:
                        if (transition == null)
                        {
                            currentPart++;
                            if (currentPart >= Credits.Count * 2)
                            {
                                currentPart = 0;
                                Destroy(CreditsObject.gameObject);
                                ShowCurrentImage();
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
                            CreditsObject.color = PaletteController.Current.BackgroundPalettes[0][1];
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
                            ImageParts[currentPart].GetComponent<PalettedSprite>().UpdatePalette();
                            lastCheckedCurrent = transition.Current;
                        }
                        break;
                    case State.ImageHold:
                        count += Time.deltaTime;
                        if (count >= HoldTime)
                        {
                            count -= HoldTime;
                            currentPart++;
                            if (currentPart >= ImageParts.Count)
                            {
                                PressStart.SetActive(true);
                                enabled = false;
                            }
                            else
                            {
                                ShowCurrentImage();
                                state = State.ImageShowing;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private void ShowCurrentCredit()
    {
        transition = PaletteController.Current.TransitionTo(true, 0, currentPart % 2 == 0 ? CreditsColor : creditsReverse, Speed);
        CreditsObject.text = Credits[currentPart / 2];
        CreditsObject.color = currentPart % 2 == 0 ? Color.black : Color.white;
        lastCheckedCurrent = transition.Current;
    }
    private void ShowCurrentImage()
    {
        transition = PaletteController.Current.TransitionTo(true, currentPart, ImagePalettes[currentPart], Speed);
        ImageParts[currentPart].gameObject.SetActive(true);
        lastCheckedCurrent = transition.Current;
    }
    public void Restore()
    {
        fadeOut = false;
        enabled = false;
        gameObject.SetActive(true);
        for (int i = 0; i < ImageParts.Count; i++)
        {
            PaletteController.Current.BackgroundPalettes[i] = ImagePalettes[i].Clone();
            ImageParts[i].GetComponent<PalettedSprite>().UpdatePalette();
        }
    }
}

/*
 * Replicate the NES Fire Emblem opening:
 * 1. Year-company-presents
 * 2. Fade in the logo
 * 3. Show the menu
 */ 
