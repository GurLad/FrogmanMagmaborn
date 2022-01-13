using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspendController : MonoBehaviour
{
    public static SuspendController Current;

    public SuspendDataConversationPlayer SuspendDataConversationPlayer;
    public SuspendDataGameController SuspendDataGameController;

    private void Awake()
    {
        Current = this;
    }

    public void SaveToSuspendData()
    {
        SuspendDataConversationPlayer = ConversationPlayer.Current.SaveToSuspendData();
        SuspendDataGameController = GameController.Current.SaveToSuspendData();
        SavedData.Save("HasSuspendData", 1);
        SavedData.Save("SuspendData", "SuspendData", JsonUtility.ToJson(this));
    }

    public void LoadFromSuspendData()
    {
        SavedData.Save("HasSuspendData", 0); // No need to delete the data - we can just say that there is none
        SavedData.SaveAll();
        JsonUtility.FromJsonOverwrite(SavedData.Load<string>("SuspendData", "SuspendData"), this);
        ConversationPlayer.Current.LoadFromSuspendData(SuspendDataConversationPlayer);
        GameController.Current.LoadFromSuspendData(SuspendDataGameController);
    }
}
