using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleKnowledgeMenuItem : KnowledgeMenuItem
{
    protected override void ShowBoughtIndicator()
    {
        BoughtIndicator.gameObject.SetActive(true);
        BoughtIndicator.sprite = Controller.SetUpgradeActive(Upgrade, Upgrade.Active);
    }

    protected override void ChangeActiveStatus()
    {
        BoughtIndicator.sprite = Controller.SetUpgradeActive(Upgrade, !Upgrade.Active);
    }

    protected override void Buy()
    {
        BoughtIndicator.sprite = Controller.BuyUpgrade(Upgrade);
        BoughtIndicator.gameObject.SetActive(true);
    }
}
