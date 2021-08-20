using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMapEventListener : AGameControllerListener
{
    private ConversationData targetEvent;

    public void Init(string eventData)
    {
        targetEvent = new ConversationData(eventData);
    }

    public override void OnBeginPlayerTurn(List<Unit> units)
    {
        if (MidBattleScreen.HasCurrent)
        {
            return;
        }
        if (targetEvent.MeetsRequirements())
        {
            ConversationPlayer.Current.PlayOneShot(string.Join("\n", targetEvent.Lines));
            Destroy(this);
        }
    }

    public override void OnEndMap(List<Unit> units, bool playerWon)
    {
        Destroy(this);
    }
}
