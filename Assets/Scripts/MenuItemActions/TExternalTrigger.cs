using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TExternalTrigger : Trigger
{
    public List<Trigger> Targets;
    public override void Activate()
    {
        Targets.ForEach(a => a.Activate());
    }
}
