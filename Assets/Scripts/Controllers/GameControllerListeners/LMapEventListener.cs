using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMapEventListener : AGameControllerListener
{
    public string EventData;
    private ConversationData targetEvent;

    public void Init(string eventData)
    {
        targetEvent = new ConversationData(EventData = eventData);
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

    public override void OnEndLevel(List<Unit> units, bool playerWon)
    {
        Destroy(this);
    }

    public override void OnPlayerWin(List<Unit> units)
    {
        // Do nothing
    }
}
