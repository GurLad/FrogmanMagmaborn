using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingCardsController : MidBattleScreen
{
    public List<CharacterEndingData> CharacterEndings;
    public EndingCardHolder EndingCardHolder;
    private int currentCharacter;

    public void DisplayNext()
    {
        if (currentCharacter < CharacterEndings.Count)
        {
            EndingCardData current = null;
            do
            {
                current = CharacterEndings[currentCharacter].EndingCards.Find(a => new ConversationData("~\n" + a.Requirements + "\n~\n~\n").MeetsRequirements());
            } while (current == null && ++currentCharacter < CharacterEndings.Count);
            if (current != null)
            {
                EndingCardHolder.Display(CharacterEndings[currentCharacter].CharacterName, current.Title, current.Card);
                currentCharacter++;
            }
            else
            {
                DisplayNext();
            }
        }
        else
        {
            // TBA
        }
    }

    [System.Serializable]
    public class CharacterEndingData
    {
        public string CharacterName;
        public List<EndingCardData> EndingCards;
    }

    [System.Serializable]
    public class EndingCardData
    {
        public string Requirements;
        public string Title;
        [TextArea]
        public string Card;
    }
}
