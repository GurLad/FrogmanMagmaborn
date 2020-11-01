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
    private bool bought;
    private void Start()
    {
        if (bought = Controller.HasUpgrade(Upgrade))
        {
            BoughtIndicator.gameObject.SetActive(true);
            BoughtIndicator.sprite = Controller.SetActive(Upgrade, Upgrade.Active);
        }
    }
    public override void Select()
    {
        base.Select();
        Controller.Description.text = Upgrade.Description;
    }
    public override void Activate()
    {
        if (bought)
        {
            if (Upgrade.Type == KnowledgeUpgradeType.Toggle)
            {
                 BoughtIndicator.sprite = Controller.SetActive(Upgrade, !Upgrade.Active);
            }
        }
        else
        {
            if (Controller.Knowledge >= Upgrade.Cost)
            {
                Controller.Buy(Upgrade);
                BoughtIndicator.gameObject.SetActive(true);
            }
        }
    }
}
