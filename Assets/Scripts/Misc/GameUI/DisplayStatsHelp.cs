using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayStatsHelp : Trigger
{
    [TextArea]
    public string Help;
    [SerializeField]
    private Text text;
    [SerializeField]
    private MenuController menu;
    private MidBattleScreen caller = null;

    private void Reset()
    {
        text = GetComponentInChildren<Text>();
        menu = GetComponentInChildren<MenuController>();
    }

    private void Start()
    {
        Help = Help.Replace("Strength", "Str".ToColoredString(1) + "ength");
        Help = Help.Replace("Endurance", "End".ToColoredString(1) + "urance");
        Help = Help.Replace("Pierce", "Pi".ToColoredString(2) + "e" + "r".ToColoredString(2) + "ce");
        Help = Help.Replace("Armor", "Arm".ToColoredString(2) + "or");
        Help = Help.Replace("Precision", "Pre".ToColoredString(0) + "cision");
        Help = Help.Replace("Evasion", "Eva".ToColoredString(0) + "sion");
        text.text = Help;
    }

    public override void Activate()
    {
        if (caller != null)
        {
            Destroy(gameObject);
            MidBattleScreen.Set(caller, true);
            caller = null;
        }
        else
        {
            throw Bugger.Error("Cannot activate DisplayStatsHelp without a previous MidBattleScreen", false);
        }
    }

    public void Activate(MidBattleScreen caller, Transform canvas)
    {
        MidBattleScreen.Set(caller, false);
        DisplayStatsHelp display = Instantiate(this, canvas).GetComponent<DisplayStatsHelp>();
        display.menu.Begin();
        display.caller = caller;
    }
}
