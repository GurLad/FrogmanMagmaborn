using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpController : MidBattleScreen
{
    [Header("Stats")]
    public float LevelUpObjectHeight = 32;
    [Header("Objects")]
    public PortraitHolder PortraitHolder;
    public Text UnitInfo;
    public BattleStatsPanel StatInfo;
    public LevelUpObject BaseLevelUpObject;
    [Header("Help")]
    public Text HelpDisplay;
    public DisplayStatsHelp StatsHelp;
    private List<Unit> players;
    private int numOptions;
    private int currentUnitID = -1;
    private List<LevelUpObject> levelUpObjects;
    private int selected;
    private int previousSign;
    public void Init(List<Unit> players)
    {
        this.players = players;
        HelpDisplay.text = HelpDisplay.text.Replace("Select", Control.DisplayShortButtonName(Control.CB.Select));
        numOptions = GameCalculations.NumLevelUpOptions;
        levelUpObjects = new List<LevelUpObject>();
        for (int i = 0; i < numOptions; i++)
        {
            LevelUpObject levelUpObject = Instantiate(BaseLevelUpObject, BaseLevelUpObject.transform.parent);
            levelUpObject.RectTransform.anchoredPosition -= new Vector2(0, LevelUpObjectHeight * i);
            levelUpObject.gameObject.SetActive(true);
            levelUpObjects.Add(levelUpObject);
        }
        NextUnit();
    }
    public void NextUnit()
    {
        currentUnitID++;
        if (currentUnitID >= players.Count)
        {
            // Pause the game so nothing accidently breaks
            enabled = false;
            GameController.Current.enabled = false;
            // Prepare the actions
            System.Action postFadeOut = () =>
            {
                Destroy(transform.parent.gameObject);
                GameController.Current.enabled = true;
                GameController.Current.transform.parent.gameObject.SetActive(true);
                Set(this, false);
                ConversationPlayer.Current.Play(GameController.Current.CreateLevel(), true);
            };
            // Begin the fade
            PaletteController.Current.FadeOut(postFadeOut);
            return;
        }
        Unit unit = players[currentUnitID];
        PortraitHolder.Portrait = PortraitController.Current.FindPortrait(unit.Name);
        UnitInfo.text = "\n" + unit + "\n\n\nLevel:" + unit.Level + "\n\n";
        int numStats = GameCalculations.StatsPerLevel(unit.TheTeam, unit.Name);
        // Technically should be combination, but adding factorial etc. seems like an overkill, considering the max amount of options is 3
        int options = unit.Stats.Base.SumGrowths <= numStats ? 1 : unit.Stats.Base.SumGrowths; // Pick min amount of options, see above
        for (int i = 0; i < numOptions; i++)
        {
            Stats current;
            do
            {
                current = unit.Stats.Base.GetLevelUp(numStats);
            } while (levelUpObjects.Find(a => a.Stats == current) != null || i >= options);
            levelUpObjects[i].Stats = current;
            levelUpObjects[i].Text.text = levelUpObjects[i].Stats.ToColoredString().Replace("\n", "\n\n");
        }
        levelUpObjects[selected].PalettedSprite.Palette = 3;
        selected = 0;
        levelUpObjects[0].PalettedSprite.Palette = 0;
        StatInfo.Display(unit, levelUpObjects[0].Stats);
    }
    private void Update()
    {
        if (!IsCurrent())
        {
            return;
        }
        if (Control.GetButtonDown(Control.CB.Select))
        {
            StatsHelp.Activate(this, transform);
        }
        else if (Control.GetButtonDown(Control.CB.A))
        {
            players[currentUnitID].Stats.Base += levelUpObjects[selected].Stats;
            NextUnit();
            SystemSFXController.Play(SystemSFXController.Type.MenuSelect);
        }
        else if (Control.GetAxisInt(Control.Axis.Y) != 0 && Control.GetAxisInt(Control.Axis.Y) != previousSign)
        {
            levelUpObjects[selected].PalettedSprite.Palette = 3;
            selected += -Control.GetAxisInt(Control.Axis.Y) + numOptions;
            selected %= numOptions;
            levelUpObjects[selected].PalettedSprite.Palette = 0;
            StatInfo.Display(players[currentUnitID], levelUpObjects[selected].Stats);
            SystemSFXController.Play(SystemSFXController.Type.MenuMove);
        }
        previousSign = Control.GetAxisInt(Control.Axis.Y);
    }
}
