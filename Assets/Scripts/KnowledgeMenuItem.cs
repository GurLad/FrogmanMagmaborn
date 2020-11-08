using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnowledgeMenuItem : MenuItem
{
    public Image BoughtIndicator;
    [HideInInspector]
    public KnowledgeUpgrade Upgrade;
    [HideInInspector]
    public KnowledgeController Controller;
    private PalettedSprite IndicatorPalette;
    private void Start()
    {
        if (Upgrade.Type != KnowledgeUpgradeType.Toggle)
        {
            IndicatorPalette = BoughtIndicator.gameObject.GetComponent<PalettedSprite>();
        }
        if (Upgrade.Bought)
        {
            BoughtIndicator.gameObject.SetActive(true);
            if (Upgrade.Type == KnowledgeUpgradeType.Toggle)
            {
                BoughtIndicator.sprite = Controller.SetUpgradeActive(Upgrade, Upgrade.Active);
            }
            else
            {
                IndicatorPalette.Palette = Controller.SetUpgradeChoiceValue(Upgrade, Upgrade.ChoiceValue);
                BoughtIndicator.sprite = Controller.ChoiceSprites[Upgrade.ChoiceValue - 1];
            }
        }
    }
    public override void Select()
    {
        base.Select();
        Controller.Description.text = Upgrade.Description;
        Controller.Cost.text = "Cost:" + Upgrade.Cost.ToString().PadLeft(7);
        Controller.Cost.gameObject.SetActive(!Upgrade.Bought);
    }
    public override void Activate()
    {
        if (Upgrade.State != KnowledgeUpgrade.UpgradeState.Available)
        {
            if (Upgrade.Type == KnowledgeUpgradeType.Toggle)
            {
                BoughtIndicator.sprite = Controller.SetUpgradeActive(Upgrade, !Upgrade.Active);
            }
            else
            {
                IndicatorPalette.Palette = Controller.SetUpgradeChoiceValue(Upgrade, Mathf.Max(1, (Upgrade.ChoiceValue + 1) % (Upgrade.NumChoices + 1)));
                BoughtIndicator.sprite = Controller.ChoiceSprites[Upgrade.ChoiceValue - 1];
            }
        }
        else
        {
            if (Controller.Knowledge >= Upgrade.Cost)
            {
                if (Upgrade.Type == KnowledgeUpgradeType.Toggle)
                {
                    BoughtIndicator.sprite = Controller.BuyUpgrade(Upgrade);
                    BoughtIndicator.gameObject.SetActive(true);
                }
                else
                {
                    Controller.BuyUpgrade(Upgrade);
                    BoughtIndicator.gameObject.SetActive(true);
                    IndicatorPalette.Palette = Controller.SetUpgradeChoiceValue(Upgrade, Upgrade.ChoiceValue);
                    BoughtIndicator.sprite = Controller.ChoiceSprites[Upgrade.ChoiceValue - 1];
                }
            }
        }
    }
}
