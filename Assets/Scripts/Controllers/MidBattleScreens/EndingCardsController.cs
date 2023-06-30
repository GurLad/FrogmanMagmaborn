using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingCardsController : MidBattleScreen
{
    public List<CharacterEndingData> CharacterEndings;
    public List<Sprite> RankSprites;
    [Header("Rank palettes")]
    public List<Palette> RankPalettes = new List<Palette> { new Palette() };
    public EndingCardHolder EndingCardHolder;
    [HideInInspector]
    public PaletteController.PaletteControllerState SavedState;
    private int currentCharacter;
    private List<ProcessedEndingData> processedEndingDatas = new List<ProcessedEndingData>();

    public void Init()
    {
        for (int i = 0; i < RankPalettes.Count; i++)
        {
            PaletteController.Current.BackgroundPalettes[i].CopyFrom(RankPalettes[i]);
        }
        GameController.Current.LevelMetadata.SetPalettesFromMetadata();
        SavedState = PaletteController.Current.SaveState();
        // Sorts all unit stats, and sets the ranking of each unit. Extremely convoluted.
        List<List<KeyValuePair<int, ProcessedEndingData>>> stats = new List<List<KeyValuePair<int, ProcessedEndingData>>>();
        for (int i = 0; i < UnitStats.StatInternalNames.Length; i++)
        {
            stats[i] = new List<KeyValuePair<int, ProcessedEndingData>>();
        }
        for (int i = 0; i < CharacterEndings.Count; i++)
        {
            EndingCardData card = CharacterEndings[i].EndingCards.Find(a => new ConversationData("~\n" + a.Requirements + "\n~\n~\n").MeetsRequirements());
            if (card != null)
            {
                processedEndingDatas.Add(new ProcessedEndingData(CharacterEndings[i].CharacterName, card.Title, card.Card));
                for (int j = 0; j < stats.Count; j++)
                {
                    stats[j].Add(new KeyValuePair<int, ProcessedEndingData>(processedEndingDatas[i].Stats[j], processedEndingDatas[i]));
                }
            }
        }
        for (int i = 0; i < stats.Count; i++)
        {
            stats[i].Sort((a, b) => a.Key.CompareTo(b.Key));
            for (int j = 0; j < stats[i].Count; j++)
            {
                stats[i][j].Value.Stats.StatRankings[i].Ranking = j;
            }
        }
    }

    public void DisplayNext()
    {
        if (currentCharacter < processedEndingDatas.Count)
        {
            EndingCardHolder.Display(processedEndingDatas[currentCharacter]);
            currentCharacter++;
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

    public class ProcessedEndingData
    {
        public string CharacterName;
        public string Title;
        public string Card;
        public UnitStats Stats;

        public ProcessedEndingData(string characterName, string title, string card)
        {
            CharacterName = characterName;
            Title = title;
            Card = card;
            Stats = new UnitStats(characterName);
        }
    }

    public class UnitStats
    {
        public static string[] StatInternalNames { get; } = new string[] { "Maps", "Battle", "Kill", "Death" };
        public static string[] StatDisplayNames { get; } = new string[] { "Maps", "Fights", "Wins", "Losses" };

        public List<StatRankingPair> StatRankings = new List<StatRankingPair>();
        public int this[int index]
        {
            get
            {
                return StatRankings[index].Stat;
            }
        }

        public UnitStats(string unit)
        {
            for (int i = 0; i < StatInternalNames.Length; i++)
            {
                StatRankings.Add(new StatRankingPair(SavedData.Load("Statistics", unit + "DeathCount", 0)));
            }
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < StatInternalNames.Length; i++)
            {
                res += StatDisplayNames[i] + ": " + StatRankings[i].ToString().PadRight(3) + (i % 2 == 0 ? " " : "\n");
            }
            return res.Substring(0, res.Length - 1);
        }

        public class StatRankingPair
        {
            public int Stat, Ranking;

            public StatRankingPair(int stat)
            {
                Stat = stat;
            }
        }
    }
}
