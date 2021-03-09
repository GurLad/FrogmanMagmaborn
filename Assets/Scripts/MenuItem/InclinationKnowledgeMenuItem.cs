using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InclinationKnowledgeMenuItem : KnowledgeMenuItem
{
    protected override void ShowBoughtIndicator()
    {
        BoughtIndicator.gameObject.SetActive(true);
        IndicatorPalette.Palette = Controller.SetUpgradeChoiceValue(Upgrade, Upgrade.ChoiceValue);
        BoughtIndicator.sprite = Controller.InclinationSprites[Upgrade.ChoiceValue - 1];
    }

    protected override void ChangeActiveStatus()
    {
        IndicatorPalette.Palette = Controller.SetUpgradeChoiceValue(Upgrade, Mathf.Max(1, (Upgrade.ChoiceValue + 1) % (Upgrade.NumChoices + 1)));
        BoughtIndicator.sprite = Controller.InclinationSprites[Upgrade.ChoiceValue - 1];
    }

    protected override void Buy()
    {
        Controller.BuyUpgrade(Upgrade);
        BoughtIndicator.gameObject.SetActive(true);
        IndicatorPalette.Palette = Controller.SetUpgradeChoiceValue(Upgrade, Upgrade.ChoiceValue);
        BoughtIndicator.sprite = Controller.InclinationSprites[Upgrade.ChoiceValue - 1];
    }
}
