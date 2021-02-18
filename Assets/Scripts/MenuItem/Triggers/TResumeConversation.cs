using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TResumeConversation : Trigger
{
    public override void Activate()
    {
        ConversationPlayer.Current.Resume();
    }
}
