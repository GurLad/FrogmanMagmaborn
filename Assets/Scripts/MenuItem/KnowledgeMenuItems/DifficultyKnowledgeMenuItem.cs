using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyKnowledgeMenuItem : KnowledgeMenuItem
{
    protected override void ShowBoughtIndicator()
    {
        BoughtIndicator.gameObject.SetActive(true);
        Controller.SetUpgradeChoiceValue(Upgrade, Upgrade.ChoiceValue);
        IndicatorPalette.Palette = Upgrade.ChoiceValue - 1;
        BoughtIndicator.sprite = Controller.DifficultySprites[Upgrade.ChoiceValue - 1];
        Text = DifficultyName(Upgrade.ChoiceValue).PadRight(12);
    }

    protected override void ChangeActiveStatus()
    {
        Controller.SetUpgradeChoiceValue(Upgrade, Mathf.Max(1, (Upgrade.ChoiceValue + 1) % (Upgrade.NumChoices + 1)));
        IndicatorPalette.Palette = Upgrade.ChoiceValue - 1;
        BoughtIndicator.sprite = Controller.DifficultySprites[Upgrade.ChoiceValue - 1];
        Text = DifficultyName(Upgrade.ChoiceValue).PadRight(12);
        Select();
    }

    protected override void Buy()
    {
        Controller.BuyUpgrade(Upgrade);
        BoughtIndicator.gameObject.SetActive(true);
        Controller.SetUpgradeChoiceValue(Upgrade, Upgrade.ChoiceValue);
        IndicatorPalette.Palette = Upgrade.ChoiceValue - 1;
        BoughtIndicator.sprite = Controller.DifficultySprites[Upgrade.ChoiceValue - 1];
    }

    public override void Select()
    {
        base.Select();
        Controller.Description.text = "Change the game's difficulty.\n\nCurrent: " +
            DifficultyName(Upgrade.ChoiceValue) + "\n\n" + DifficultyDescription(Upgrade.ChoiceValue);
        Text = DifficultyName(Upgrade.ChoiceValue).PadRight(12);
    }

    private string DifficultyName(int id)
    {
        return id switch
        {
            1 => "Normal",
            2 => "Hard",
            3 => "Insane",
            _ => null
        };
    }

    private string DifficultyDescription(int id)
    {
        return id switch
        {
            1 => "Your units gain +2 extra levels. Recommended.",
            2 => "Your units gain an extra level.",
            3 => "Your units do not get extra levels. Not recommended.",
            _ => null
        };
    }
}
