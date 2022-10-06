using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPlayConversation : Trigger
{
    public string Data;

    public override void Activate()
    {
        ConversationPlayer.Current.PlayOneShot(Data);
    }
}
