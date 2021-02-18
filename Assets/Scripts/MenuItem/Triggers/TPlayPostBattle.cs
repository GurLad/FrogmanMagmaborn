using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPlayPostBattle : Trigger
{
    public override void Activate()
    {
        ConversationPlayer.Current.PlayPostBattle();
    }
}
