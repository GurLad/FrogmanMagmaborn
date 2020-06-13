using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusScreenController : MidBattleScreen
{
    public Text Name;
    public Text Stats;
    public Image Icon;
    public RectTransform HealthbarFull;
    public RectTransform HealthbarEmpty;
    public List<PalettedSprite> PaletteSprites;
    public void Show(Unit unit)
    {
        Name.text = unit.Name + "\nHP:" + unit.Health + "/" + unit.Stats.MaxHP;
        Stats.text = unit.Stats.ToString();
        Icon.sprite = unit.Icon;
        HealthbarFull.sizeDelta = new Vector2(unit.Health * 4, 8);
        HealthbarEmpty.sizeDelta = new Vector2(unit.Stats.MaxHP * 4, 8);
        foreach (var item in PaletteSprites)
        {
            item.Palette = (int)unit.TheTeam;
        }
    }
    private void Update()
    {
        if (Control.GetButtonUp(Control.CB.B))
        {
            Quit();
        }
    }
}
