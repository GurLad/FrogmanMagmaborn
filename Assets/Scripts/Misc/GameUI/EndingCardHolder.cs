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
    [Header("Objects")]
    public EndingCardsController EndingCardsController;
    public PortraitHolder PortraitHolder;
    public Text Title;
    public Text Stats;
    public Text Card;
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
                if (count >= 1)
                {
                    count--;
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

    public void Display(string characterName, string title, string card)
    {
        Title.text = title;
        Card.text = "";
        DisplayUnitStats(characterName);
        PortraitHolder.Portrait = PortraitController.Current.FindPortrait(characterName);
        GameController.Current.LevelMetadata.SetPalettesFromMetadata();
        PaletteController.Current.FadeIn(() => { cardText = card.FindLineBreaks(LineWidth); state = State.Writing; });
    }

    private void DisplayUnitStats(string unit)
    {
        Stats.text = "Maps: " + SavedData.Load("Statistics", unit + "MapsCount", 0).ToString().PadRight(3) +
            "  Fights: " + SavedData.Load("Statistics", unit + "BattleCount", 0).ToString().PadRight(3) +
            "\nWins: " + SavedData.Load("Statistics", unit + "KillCount", 0).ToString().PadRight(3) +
            "  Losses: " + SavedData.Load("Statistics", unit + "DeathCount", 0).ToString().PadRight(3);
    }
}
