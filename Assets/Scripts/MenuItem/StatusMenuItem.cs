using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusMenuItem : MenuItem
{
    [Header("Objects")]
    public BattleStatsPanel BattleStatsPanel;
    public Text Info;
    public PortraitHolder Portrait;
    public List<PalettedSprite> PalettedSprites;
    public GameObject StatusScreen;
    public RectTransform RectTransform;
    private Unit player;
    private List<List<Unit>> unitLists;

    private void Reset()
    {
        BattleStatsPanel = GetComponentInChildren<BattleStatsPanel>();
        Portrait = GetComponentInChildren<PortraitHolder>();
        RectTransform = GetComponent<RectTransform>();
    }

    public void Init(Unit player, List<List<Unit>> unitLists)
    {
        this.player = player;
        this.unitLists = unitLists;
        BattleStatsPanel.Display(player);
        Info.text = player + "\n\nLevel:" + player.Level;
        Portrait.Portrait = player.Icon;
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
        statusScreenController.Show(player, unitLists);
        statusScreenController.TransitionToThis();
    }
}
