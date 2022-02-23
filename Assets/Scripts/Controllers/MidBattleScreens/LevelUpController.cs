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
    [HideInInspector]
    public List<Unit> Players;
    private int numOptions;
    private int currentUnitID = -1;
    private List<LevelUpObject> levelUpObjects;
    private int selected;
    private int previousSign;
    private void Start()
    {
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
        if (currentUnitID >= Players.Count)
        {
            GameController.Current.CreateLevel();
            Quit();
            return;
        }
        Unit unit = Players[currentUnitID];
        PortraitHolder.Portrait = PortraitController.Current.FindPortrait(unit.Name);
        UnitInfo.text = "\n" + unit.Name + "\n\n\nLevel:" + unit.Level + "\n\n";
        for (int i = 0; i < numOptions; i++)
        {
            Stats current;
            int numStats = GameCalculations.StatsPerLevel(Team.Player, unit.Name);
            do
            {
                current = unit.Stats.GetLevelUp(numStats);
            } while (levelUpObjects.Find(a => a.Stats == current) != null);
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
            Players[currentUnitID].Stats += levelUpObjects[selected].Stats;
            NextUnit();
        }
        else if (Control.GetAxisInt(Control.Axis.Y) != 0 && Control.GetAxisInt(Control.Axis.Y) != previousSign)
        {
            levelUpObjects[selected].PalettedSprite.Palette = 3;
            selected += -Control.GetAxisInt(Control.Axis.Y) + numOptions;
            selected %= numOptions;
            levelUpObjects[selected].PalettedSprite.Palette = 0;
            StatInfo.Display(Players[currentUnitID], levelUpObjects[selected].Stats);
        }
        previousSign = Control.GetAxisInt(Control.Axis.Y);
    }
}
