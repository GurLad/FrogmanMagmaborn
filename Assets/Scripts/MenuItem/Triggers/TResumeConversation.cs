using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TResumeConversation : Trigger
{
    public bool SoftResume = false;

    public override void Activate()
    {
        if (SoftResume)
        {
            ConversationPlayer.Current.SoftResume();
        }
        else
        {
            ConversationPlayer.Current.Resume();
        }
    }
}
