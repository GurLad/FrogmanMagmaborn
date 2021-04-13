using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class KnowledgeMenuItem : MenuItem
{
    public Image BoughtIndicator;
    [HideInInspector]
    public KnowledgeMenuController.KnowledgeUpgrade Upgrade;
    [HideInInspector]
    public KnowledgeMenuController Controller;
    protected PalettedSprite IndicatorPalette;
    private void Start()
    {
        IndicatorPalette = BoughtIndicator.gameObject.GetComponent<PalettedSprite>();
        if (Upgrade.Bought)
        {
            ShowBoughtIndicator();
        }
    }
    public override void Select()
    {
        base.Select();
        Controller.Description.text = Upgrade.Description;
        Controller.Cost.text = Upgrade.Bought ? "Bought" : ("Cost:" + Upgrade.Cost.ToString().PadLeft(7));
    }
    public override void Activate()
    {
        if (Upgrade.State != KnowledgeMenuController.UpgradeState.Available)
        {
            ChangeActiveStatus();
        }
        else
        {
            if (Controller.Knowledge >= Upgrade.Cost)
            {
                Controller.Cost.text = "Bought";
                Buy();
            }
        }
    }
    protected abstract void Buy();
    protected abstract void ChangeActiveStatus();
    protected abstract void ShowBoughtIndicator();
}
