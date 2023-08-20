using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingCardHolder : MonoBehaviour
{
    private enum State { Idle, Writing, Waiting }

    [Header("Vars")]
    public float Speed;
    public float WaitTime;
    public int LineWidth = 30;
    [Header("SFX")]
    public AudioClip TypeSound;
    [Header("Objects")]
    public EndingCardsController EndingCardsController;
    public PortraitHolder PortraitHolder;
    public Text Title;
    public Text Stats;
    public Text Card;
    public List<PalettedSprite> RankPalettedSprites;
    public List<Image> RankImages;
    private State state;
    private string cardText = "";
    private int currentLetter = 0;
    private float count;

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;
            case State.Writing:
                count += Time.deltaTime * Speed;
                if (Control.GetButton(Control.CB.Start))
                {
                    count += 2 * Time.deltaTime * Speed;
                }
                if (count >= 1)
                {
                    count--;
                    // A simpler version of ConversationPlayer's PlayLetter, to sound more basic/artifical (typing/writing sound vs. voice)
                    if (count % 2 == 0 && cardText.ToLower()[currentLetter] >= 'a' && cardText.ToLower()[currentLetter] <= 'z')
                    {
                        SoundController.PlaySound(TypeSound);
                    }
                    Card.text += cardText[currentLetter++];
                    if (currentLetter >= cardText.Length)
                    {
                        cardText = "";
                        currentLetter = 0;
                        count = 0;
                        state = State.Waiting;
                    }
                }
                break;
            case State.Waiting:
                count += Time.deltaTime;
                if (count >= WaitTime)
                {
                    count = 0;
                    state = State.Idle;
                    PaletteController.Current.FadeOut(() => EndingCardsController.DisplayNext());
                }
                break;
            default:
                break;
        }
    }

    public void Display(EndingCardsController.ProcessedEndingData endingData)
    {
        Title.text = endingData.Title;
        Card.text = "";
        Stats.text = endingData.Stats.ToString();
        for (int i = 0; i < RankImages.Count; i++)
        {
            int ranking = endingData.Stats.StatRankings[i].Ranking;
            if (ranking < EndingCardsController.RankSprites.Count)
            {
                RankImages[i].gameObject.SetActive(true);
                RankImages[i].sprite = EndingCardsController.RankSprites[ranking];
                RankPalettedSprites[i].Palette = ranking;
            }
            else
            {
                RankImages[i].gameObject.SetActive(false);
            }
        }
        PaletteController.Current.LoadState(EndingCardsController.SavedState);
        PortraitHolder.Portrait = PortraitController.Current.FindPortrait(endingData.CharacterName);
        PaletteController.Current.FadeIn(() => { cardText = endingData.Card.FindLineBreaks(LineWidth); state = State.Writing; });
    }
}
