using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelMetadata
{
    public TeamData[] TeamDatas = new TeamData[3];
    public string MusicName;

    public LevelMetadata()
    {
        for (int i = 0; i < 3; i++)
        {
            TeamDatas[i] = new TeamData();
            TeamDatas[i].Palette = new Palette();
            TeamDatas[i].Palette.Colors[3] = new Color(1, 0, 1, 0);
            TeamDatas[i].Name = ((Team)i).ToString();
            TeamDatas[i].PortraitLoadingMode = (PortraitLoadingMode)i;
            TeamDatas[i].PlayerControlled = i == 0;
        }
    }

    [System.Serializable]
    public class TeamData
    {
        public Palette Palette;
        public string Name;
        public bool PlayerControlled; // Functionality TBA
        public PortraitLoadingMode PortraitLoadingMode;
        public AIPriorities AI;
    }
}