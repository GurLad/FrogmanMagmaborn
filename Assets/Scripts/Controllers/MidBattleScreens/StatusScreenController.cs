using System.Collections;
using System.Collections.Generic;
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
    public List<PalettedSprite> PaletteSprites;
    public void Show(Unit unit)
    {
        Name.text = unit.ToString() + "\nHP:" + unit.Health + "/" + unit.Stats.MaxHP + "\nLevel:" + unit.Level;
        Stats.text = unit.Stats.ToString();
        Weapon.text = unit.Weapon.ToString();
        Status.text = "Team:" + unit.TheTeam.ToString().PadRight(7) + (unit.TheTeam != Team.Player ? ("\nA.I.:" + unit.AIType.ToString().PadRight(7)) : "\n") + "\nCond:" + unit.State().PadRight(7);
        BattleStats.Display(unit);
        Inclination.Display(unit);
        Icon.Portrait = unit.Icon;
        HealthbarFull.sizeDelta = new Vector2(unit.Health * 4, 8);
        HealthbarEmpty.sizeDelta = new Vector2(unit.Stats.MaxHP * 4, 8);
        foreach (var item in PaletteSprites)
        {
            item.Palette = (int)unit.TheTeam;
        }
    }
    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.B))
        {
            Quit();
        }
    }
}
