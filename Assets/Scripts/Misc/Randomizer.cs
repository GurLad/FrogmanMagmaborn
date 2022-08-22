using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomizer
{
    public Randomizer(int seed)
    {
        Random.InitState(seed);
    }

    public Randomizer() { }

    public void Randomize(PortraitController portraitController, UnitClassData unitClassData, MapController mapController, LevelMetadataController levelMetadataController)
    {
        Dictionary<string, string> internalNameReplacements = new Dictionary<string, string>();
        Dictionary<string, string> displayNameReplacements = new Dictionary<string, string>();
        // Begin with the portraits, as they'll determine everything else
        List<Portrait> origin = portraitController.Portraits.ConvertAll(a => a.Clone()); // Clone list
        List<Portrait> shuffled = portraitController.Portraits.Shuffle();
        Bugger.Info(string.Join(", ", origin.ConvertAll(a => a.Name)));
        Bugger.Info(string.Join(", ", shuffled.ConvertAll(a => a.Name)));
        for (int i = 0; i < origin.Count; i++)
        {
            internalNameReplacements.Add(origin[i].Name, shuffled[i].Name);
            shuffled[i].Name = origin[i].Name;
            if (!displayNameReplacements.ContainsKey(origin[i].TheDisplayName))
            {
                displayNameReplacements.Add(origin[i].TheDisplayName, shuffled[i].TheDisplayName);
            }
        }
        // Units
        foreach (UnitData unitData in unitClassData.UnitDatas)
        {
            // Always replace fliers with other fliers
            ClassData oldClass = unitClassData.ClassDatas.Find(a => a.Name == unitData.Class);
            List<ClassData> options = unitClassData.ClassDatas.FindAll(a => a.Flies == oldClass.Flies);
            ClassData newClass = options[Random.Range(0, options.Count)];
            unitData.DisplayName = shuffled.Find(a => a.Name == unitData.Name).TheDisplayName;
            unitData.Class = newClass.Name;
            unitData.Inclination = (Inclination)Random.Range(0, 3);
            // Half-randomize growths: two completely random levels, then add the base class' growths - 1, then the inclination bonus
            UnitClassData.GrowthsStruct growths = new UnitClassData.GrowthsStruct();
            for (int i = 0; i < 6; i++)
            {
                growths.Values[i] += newClass.Growths.Values[i] - 1;
                growths.Values[Random.Range(0, 6)]++;
                if (i / 2 == (int)unitData.Inclination)
                {
                    growths.Values[i]++;
                }
            }
        }
        // Maps
        foreach (Map map in mapController.Maps)
        {
            if (displayNameReplacements.ContainsKey(map.ObjectiveData))
            {
                map.ObjectiveData = displayNameReplacements[map.ObjectiveData];
            }
            foreach (MapController.UnitPlacementData unit in map.Units)
            {
                if (levelMetadataController[map.LevelNumber].TeamDatas[(int)unit.Team].PortraitLoadingMode != PortraitLoadingMode.Name)
                {
                    unit.Class = unitClassData.ClassDatas[Random.Range(0, unitClassData.ClassDatas.Count)].Name;
                }
            }
        }
    }
}
