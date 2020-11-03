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
    private void Start()
    {
        if (Upgrade.Bought)
        {
            BoughtIndicator.gameObject.SetActive(true);
            BoughtIndicator.sprite = Controller.SetUpgradeActive(Upgrade, Upgrade.Active);
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
        }
        else
        {
            if (Controller.Knowledge >= Upgrade.Cost)
            {
                BoughtIndicator.sprite = Controller.BuyUpgrade(Upgrade);
                BoughtIndicator.gameObject.SetActive(true);
            }
        }
    }
}
