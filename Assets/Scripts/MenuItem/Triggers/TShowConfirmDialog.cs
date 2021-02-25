using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TShowConfirmDialog : Trigger
{
    public GameObject YesAction;
    public MenuController ThisMenu;
    public TExternalTrigger YesButton;
    public TShowMenu NoButton;

    public override void Activate()
    {
        YesButton.Targets = new List<Trigger>(YesAction.GetComponents<Trigger>());
        NoButton.Target = ThisMenu;
    }
}
