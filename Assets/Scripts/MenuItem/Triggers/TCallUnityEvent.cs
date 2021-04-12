using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TCallUnityEvent : Trigger
{
    [SerializeField]
    public UnityEvent Event;

    public override void Activate()
    {
        Event.Invoke();
    }
}
