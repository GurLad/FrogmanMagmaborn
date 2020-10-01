using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TShowConfirmDialog : Trigger
{
    public GameObject YesAction;
    public GameObject ThisMenu;
    public TExternalTrigger YesButton;
    public TEnableDisable NoButton;

    public override void Activate()
    {
        YesButton.Targets = new List<Trigger>(YesAction.GetComponents<Trigger>());
        NoButton.Target = ThisMenu;
    }
}
