using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusMenuItem : MenuItem
{
    [Header("Objects")]
    public BattleStatsPanel BattleStatsPanel;
    public Text Name;
    public AdvancedSpriteSheetAnimationUI ClassIcon;
    public List<PalettedSprite> PalettedSprites;
    public GameObject StatusScreen;
    public RectTransform RectTransform;
    [HideInInspector]
    public MenuController Menu;
    private Unit player;
    private List<List<Unit>> unitLists;

    private void Reset()
    {
        BattleStatsPanel = GetComponentInChildren<BattleStatsPanel>();
        ClassIcon = GetComponentInChildren<AdvancedSpriteSheetAnimationUI>();
        RectTransform = GetComponent<RectTransform>();
    }

    public void Init(Unit player, List<List<Unit>> unitLists)
    {
        this.player = player;
        this.unitLists = unitLists;
        BattleStatsPanel.Display(player, false);
        Name.text = player.ToString();
        ClassData classData = GameController.Current.UnitClassData.ClassDatas.Find(a => a.Name == player.Class);
        classData.SetToClassIcon(ClassIcon);
        PalettedSprites.ForEach(a => a.Awake());
    }

    public override void Select()
    {
        PalettedSprites.ForEach(a => a.Palette = (int)player.TheTeam);
    }

    public override void Unselect()
    {
        PalettedSprites.ForEach(a => a.Palette = 3);
    }

    public override void Activate()
    {
        StatusScreenController statusScreenController = Instantiate(StatusScreen).GetComponentInChildren<StatusScreenController>();
        statusScreenController.PreviousScreen = Menu;
        statusScreenController.Show(player, unitLists);
        statusScreenController.TransitionToThis();
    }
}
