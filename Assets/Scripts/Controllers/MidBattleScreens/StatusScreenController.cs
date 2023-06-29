using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class StatusScreenController : MidBattleScreen
{
    public Text Name;
    public Text Stats;
    public Text Weapon;
    public Text Status;
    public BattleStatsPanel BattleStats;
    public InclinationPanel Inclination;
    public PortraitHolder Icon;
    public RectTransform HealthbarFull;
    public RectTransform HealthbarEmpty;
    public AdvancedSpriteSheetAnimationUI ClassIcon;
    public List<PalettedSprite> PaletteSprites;
    [HideInInspector]
    public List<List<Unit>> UnitLists;
    [HideInInspector]
    public bool ShowingHelpInfo;
    [HideInInspector]
    public MidBattleScreen PreviousScreen;
    private int currentTeam;
    private int currentUnit;
    private Unit currentUnitObject;
    private Vector2Int previousDir;

    public void Show(Unit unit, List<List<Unit>> unitLists)
    {
        UnitLists = unitLists;
        currentTeam = (int)unit.TheTeam;
        //Bugger.Info("Unit: " + unit + ", pos: " + unit.Pos + ", all: " + string.Join(", ", UnitLists[currentTeam]));
        Show(unit, UnitLists[currentTeam].FindIndex(a => a.Pos == unit.Pos));
        SystemSFXController.Play(SystemSFXController.Type.LongSelect);
    }

    private void Show(Unit unit, int index)
    {
        Name.text = unit.ToString() + "\nHP:" + unit.Health + "/" + unit.Stats.Base.MaxHP + "\nLevel:" + unit.Level;
        Stats.text = unit.Stats.Base.ToString(6);
        Weapon.text = unit.Weapon.ToString();
        Status.text = "Team:" + unit.TheTeam.Name().PadRight(7) + (!unit.TheTeam.PlayerControlled() ? ("\nA.I.:" + unit.AIType.ToString().PadRight(7)) : "\n") + "\nCond:" + unit.State().PadRight(7);
        BattleStats.Display(unit);
        if (Inclination != null)
        {
            Inclination.Display(unit);
        }
        Icon.Portrait = unit.Icon;
        HealthbarFull.sizeDelta = new Vector2(unit.Health * 4, 8);
        HealthbarEmpty.sizeDelta = new Vector2(unit.Stats.Base.MaxHP * 4, 8);
        // Load class icon
        ClassData classData = GameController.Current.UnitClassData.ClassDatas.Find(a => a.Name == unit.Class);
        classData.SetToClassIcon(ClassIcon);
        foreach (var item in PaletteSprites)
        {
            item.Palette = (int)unit.TheTeam;
        }
        currentTeam = (int)unit.TheTeam;
        currentUnit = index;
        if (currentUnit < 0)
        {
            throw Bugger.FMError("Showing the status of a non-existent unit!");
        }
        currentUnitObject = unit;
    }

    private void Update()
    {
        if (!IsCurrent() || ShowingHelpInfo)
        {
            return;
        }
        if (Control.GetButtonDown(Control.CB.B))
        {
            GameController.Current.ForceSetCursorPos(currentUnitObject.Pos);
            if (PreviousScreen != null)
            {
                Quit(true, () => PreviousScreen.Begin());
            }
            else
            {
                Quit();
            }
            SystemSFXController.Play(SystemSFXController.Type.LongCancel);
            return;
        }
        Vector2Int input = new Vector2Int(Control.GetAxisInt(Control.Axis.X), Control.GetAxisInt(Control.Axis.Y));
        if (input.y != 0 && input.y != previousDir.y)
        {
            int index = (currentUnit + input.y + UnitLists[currentTeam].Count) % UnitLists[currentTeam].Count;
            enabled = false;
            var previousPalette = PaletteController.Current.SaveState();
            PaletteController.Current.FadeOut(() =>
            {
                PaletteController.Current.LoadState(previousPalette);
                Show(UnitLists[currentTeam][index], index);
                PaletteController.Current.FadeIn(() => enabled = true);
                SystemSFXController.Play(SystemSFXController.Type.LongMove);
            });
        }
        else if (input.x != 0 && input.x != previousDir.x)
        {
            int team = currentTeam;
            while (UnitLists[team = (team + input.x + 3) % 3].Count <= 0) { }
            int index = currentUnit % UnitLists[currentTeam = team].Count;
            int tempIndex = currentUnit; // For non-destructive back & forth between teams
            enabled = false;
            var previousPalette = PaletteController.Current.SaveState();
            PaletteController.Current.FadeOut(() =>
            {
                PaletteController.Current.LoadState(previousPalette);
                Show(UnitLists[currentTeam][index], index);
                currentUnit = tempIndex;
                PaletteController.Current.FadeIn(() => enabled = true);
                SystemSFXController.Play(SystemSFXController.Type.LongMove);
            });
        }
        if (!GameCalculations.TransitionsOn) // With transitions, players can just hold the button and it looks fine
        { 
            previousDir = new Vector2Int(Control.GetAxisInt(Control.Axis.X), Control.GetAxisInt(Control.Axis.Y));
        }
    }
}
