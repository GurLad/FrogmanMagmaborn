using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MidBattleScreen
{
    public List<MenuItem> MenuItems;
    public bool Cancelable; // For things like the main menu vs. pause menu
    private int selected;
    private int count;
    private int previousSign;
    public void Start()
    {
        MenuItems[selected = 0].Select();
        count = MenuItems.Count;
    }
    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.A))
        {
            MenuDone();
            MenuItems[selected].Activate();
            return;
        }
        if (Control.GetButtonDown(Control.CB.B) && Cancelable)
        {
            MenuDone();
            return;
        }
        if (Control.GetAxisInt(Control.Axis.Y) != 0 && Control.GetAxisInt(Control.Axis.Y) != previousSign)
        {
            MenuItems[selected].Unselect();
            selected += -Control.GetAxisInt(Control.Axis.Y) + count;
            selected %= count;
            MenuItems[selected].Select();
        }
        previousSign = Control.GetAxisInt(Control.Axis.Y);
    }
    private void MenuDone()
    {
        if (GameController.Current != null)
        {
            Quit();
        }
        else
        {
            // Generic menu done behaviour. Maybe add ContinuousTrigger/isDone?
            gameObject.SetActive(false);
        }
    }
}
