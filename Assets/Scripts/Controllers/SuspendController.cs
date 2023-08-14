using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspendController : MonoBehaviour
{
    public static SuspendController Current;

    public SuspendDataConversationPlayer SuspendDataConversationPlayer;
    public SuspendDataGameController SuspendDataGameController;
    public SuspendDataRunStatsController SuspendDataRunStatsController;

    private void Awake()
    {
        Current = this;
    }

    private void OnApplicationQuit()
    {
        RunStatsController.Current?.RecordGameEvent(RunStatsController.GameEvent.RecordPlayTime);
        SavedData.Append("PlayTime", Time.timeSinceLevelLoad);
        SaveToSuspendData();
        SavedData.SaveAll();
    }

    public void SaveToSuspendData()
    {
        SuspendDataConversationPlayer = ConversationPlayer.Current.SaveToSuspendData();
        SuspendDataGameController = GameController.Current.SaveToSuspendData();
        SuspendDataRunStatsController = RunStatsController.Current?.SaveToSuspendData();
        SavedData.Save("HasSuspendData", 1);
        SavedData.Save("SuspendData", "SuspendData", JsonUtility.ToJson(this));
        SavedData.Append("Log", "Data", "Saved & quit\n");
    }

    public void LoadFromSuspendData()
    {
        //SavedData.Save("HasSuspendData", 0); // Keep the suspend data, as we assume that it'll be successfully overwritten once the player quits
        //SavedData.SaveAll();
        JsonUtility.FromJsonOverwrite(SavedData.Load<string>("SuspendData", "SuspendData"), this);
        ConversationPlayer.Current.LoadFromSuspendData(SuspendDataConversationPlayer);
        GameController.Current.LoadFromSuspendData(SuspendDataGameController);
        RunStatsController.Current?.LoadFromSuspendData(SuspendDataRunStatsController);
    }
}
