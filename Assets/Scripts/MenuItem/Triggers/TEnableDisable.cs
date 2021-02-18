using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEnableDisable : Trigger
{
    public GameObject Target;
    public bool Enable;

    public override void Activate()
    {
        Target.SetActive(Enable);
    }
}
