using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionMenuItem : MenuItem
{
    [TextArea]
    public string Description;
    public Text DescriptionObject;
    public override void Select()
    {
        base.Select();
        DescriptionObject.text = Description;
    }
}
