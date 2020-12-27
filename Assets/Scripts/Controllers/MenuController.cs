using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MidBattleScreen
{
    public List<MenuItem> MenuItems;
    public int Selected;
    public bool Cancelable; // For things like the main menu vs. pause menu
    public bool FinishOnSelect = true;
    public Trigger CancelTrigger;
    private Text text;
    [HideInInspector]
    public Text Text
    {
        get
        {
            if (text == null)
            {
                text = GetComponentInChildren<Text>(true);
            }
            return text;
        }
    }
    private int count;
    private int previousSign;
    public void Start()
    {
        MenuItems[Selected].Select();
        count = MenuItems.Count;
    }
    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.A))
        {
            MenuDone();
            MenuItems[Selected].Activate();
            return;
        }
        if (Control.GetButtonDown(Control.CB.B) && Cancelable)
        {
            MenuDone();
            if (CancelTrigger != null)
            {
                CancelTrigger.Activate();
            }
            return;
        }
        if (Control.GetAxisInt(Control.Axis.Y) != 0 && Control.GetAxisInt(Control.Axis.Y) != previousSign)
        {
            MenuItems[Selected].Unselect();
            Selected += -Control.GetAxisInt(Control.Axis.Y) + count;
            Selected %= count;
            MenuItems[Selected].Select();
        }
        previousSign = Control.GetAxisInt(Control.Axis.Y);
    }
    private void MenuDone()
    {
        if (FinishOnSelect)
        {
            // Generic menu done behaviour. Maybe add ContinuousTrigger/isDone?
            MidBattleScreen.Set(this, false);
            gameObject.SetActive(false);
        }
    }
}
