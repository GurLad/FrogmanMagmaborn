using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingCardsController : MidBattleScreen
{
    public List<GlobalEndingData> GlobalEndings;
    public List<CharacterEndingData> CharacterEndings;
    public List<Sprite> RankSprites;
    [Header("Rank palettes")]
    public List<Palette> RankPalettes = new List<Palette> { new Palette() };
    [Header("Objects")]
    public EndingCardHolder EndingCardHolder;
    public EndingScrollingTextHolder EndingScrollingTextHolder;
    public EndingStatsController EndingStatsController;
    [HideInInspector]
    public PaletteController.PaletteControllerState SavedState;
    private int currentCharacter = -1;
    private GlobalEndingData processedGlobalEndingData;
    private List<ProcessedEndingData> processedEndingDatas = new List<ProcessedEndingData>();
    private string winnerName;
    private int winnerPalette;

    public void Init(string wonName, int wonPalette)
    {
        SavedData.Save("HasSuspendData", 0); // Nobody would close the game during the ending, right?
        // Ending stats
        winnerName = wonName;
        winnerPalette = wonPalette;
        // Global ending
        processedGlobalEndingData = GlobalEndings.Find(a => new ConversationData("~\n" + a.Requirements + "\n~\n~\n").MeetsRequirements());
        // Character endings
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
            stats.Add(new List<KeyValuePair<int, ProcessedEndingData>>());
        }
        for (int i = 0; i < CharacterEndings.Count; i++)
        {
            EndingCardData card = CharacterEndings[i].EndingCards.Find(a => new ConversationData("~\n" + a.Requirements + "\n~\n~\n").MeetsRequirements());
            if (card != null)
            {
                ProcessedEndingData processedEndingData = new ProcessedEndingData(CharacterEndings[i].Name, card.Title, card.Card);
                processedEndingDatas.Add(processedEndingData);
                for (int j = 0; j < stats.Count; j++)
                {
                    stats[j].Add(new KeyValuePair<int, ProcessedEndingData>(processedEndingData.Stats[j], processedEndingData));
                }
            }
        }
        for (int i = 0; i < stats.Count; i++)
        {
            stats[i].Sort((a, b) => a.Key.CompareTo(b.Key));
            stats[i].Reverse();
            for (int j = 0; j < stats[i].Count; j++)
            {
                stats[i][j].Value.Stats.StatRankings[i].Ranking = j;
            }
        }
        processedEndingDatas.Sort((a, b) => a.Stats.MVPValue().CompareTo(b.Stats.MVPValue()));
    }

    public void DisplayNext()
    {
        if (currentCharacter < 0)
        {
            EndingScrollingTextHolder.Display(processedGlobalEndingData);
            currentCharacter++;
        }
        else if (currentCharacter < processedEndingDatas.Count)
        {
            EndingScrollingTextHolder.gameObject.SetActive(false);
            EndingCardHolder.gameObject.SetActive(true);
            EndingCardHolder.Display(processedEndingDatas[currentCharacter]);
            currentCharacter++;
        }
        else
        {
            EndingCardHolder.gameObject.SetActive(false);
            EndingStatsController.gameObject.SetActive(true);
            MidBattleScreen.Set(this, false);
            MidBattleScreen.Set(EndingStatsController, true);
            EndingStatsController.Display(winnerName, winnerPalette);
            GameController.Current.LevelMetadata.SetPalettesFromMetadata();
            PaletteController.Current.FadeIn(null, 30 / 4);
        }
    }

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        // Load json
        string json = FrogForgeImporter.LoadTextFile("CharacterEndings.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("CharacterEndings"), this);
        json = FrogForgeImporter.LoadTextFile("GlobalEndings.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("GlobalEndings"), this);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif

    [System.Serializable]
    public class GlobalEndingData
    {
        public string Requirements;
        [TextArea]
        public string Text;
    }

    [System.Serializable]
    public class CharacterEndingData
    {
        public string Name;
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
                StatRankings.Add(new StatRankingPair(SavedData.Load("Statistics", unit + StatInternalNames[i] + "Count", 0)));
            }
        }

        public int MVPValue()
        {
            return GameCalculations.MVPValue(this[0], this[1], this[2], this[3]);
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < StatInternalNames.Length; i++)
            {
                res += StatDisplayNames[i] + ": " + StatRankings[i].Stat.ToString().PadRight(3) + (i % 2 == 0 ? " " : "\n");
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
