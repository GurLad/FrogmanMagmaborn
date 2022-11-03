using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPlayConversation : Trigger
{
    public ConversationData Conversation;
    public string Data;
    public AdvancedSpriteSheetAnimationUI NewIndicator;

    public override void Activate()
    {
        ConversationPlayer.Current.PlayOneShot(Data);
        Conversation.Choose(true, true);
        NewIndicator.gameObject.SetActive(false);
    }
}
