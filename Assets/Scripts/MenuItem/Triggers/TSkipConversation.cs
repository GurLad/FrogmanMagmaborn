using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSkipConversation : Trigger
{
    public override void Activate()
    {
        ConversationPlayer.Current.Skip();
    }
}
