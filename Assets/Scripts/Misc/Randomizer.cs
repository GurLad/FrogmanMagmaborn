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
        // Generate dictionaries for performance
        Dictionary<string, string> internalNameReplacements = new Dictionary<string, string>();
        Dictionary<string, string> displayNameReplacements = new Dictionary<string, string>();
        Dictionary<string, string> nameToDisplayName = new Dictionary<string, string>();
        Dictionary<string, ClassData> classes = new Dictionary<string, ClassData>();
        foreach (ClassData classData in unitClassData.ClassDatas)
        {
            classes.Add(classData.Name, classData);
        }
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
            nameToDisplayName.Add(shuffled[i].Name, shuffled[i].TheDisplayName);
        }
        Bugger.Info(string.Join(", ", nameToDisplayName.Keys));
        // Units
        foreach (UnitData unitData in unitClassData.UnitDatas)
        {
            // Always replace fliers with other fliers
            ClassData oldClass = classes[unitData.Class];
            List<ClassData> options = unitClassData.ClassDatas.FindAll(a => a.Flies == oldClass.Flies);
            ClassData newClass = options[Random.Range(0, options.Count)];
            unitData.DisplayName = shuffled.Find(a => a.Name == unitData.Name).TheDisplayName;
            unitData.Class = newClass.Name;
            unitData.Inclination = (Inclination)Random.Range(0, 3);
            // Half-randomize growths: two completely random levels, then add the base class' growths - 1, then the inclination bonus
            UnitClassData.GrowthsStruct growths = new UnitClassData.GrowthsStruct();
            for (int i = 0; i < 6; i++)
            {
                growths.Values[i] += Mathf.Max(newClass.Growths.Values[i] - 1, 0);
                growths.Values[Random.Range(0, 6)]++;
                if (i / 2 == (int)unitData.Inclination)
                {
                    growths.Values[i]++;
                }
            }
            unitData.Growths = growths;
        }
        // Maps
        foreach (Map map in mapController.Maps)
        {
            LevelMetadata levelMetadata = levelMetadataController[map.LevelNumber];
            MapController.UnitPlacementData boss = null;
            if (map.Objective == Objective.Boss)
            {
                boss = map.Units.Find(a => a.Class != "P" &&
                    (a.Class == map.ObjectiveData ||
                     (levelMetadata.TeamDatas[(int)a.Team].PortraitLoadingMode == PortraitLoadingMode.Name && nameToDisplayName[a.Class] == map.ObjectiveData)));
                if (boss != null && levelMetadata.TeamDatas[(int)boss.Team].PortraitLoadingMode == PortraitLoadingMode.Name)
                {
                    map.ObjectiveData = displayNameReplacements[map.ObjectiveData];
                }
                else if (boss == null)
                {
                    Bugger.Warning("Map " + map.Name + " has a Defeat Boss objective without the matching boss (" + map.ObjectiveData + ")!");
                }
            }
            foreach (MapController.UnitPlacementData unit in map.Units)
            {
                if (levelMetadata.TeamDatas[(int)unit.Team].PortraitLoadingMode != PortraitLoadingMode.Name)
                {
                    Bugger.Info(map.Name + ": " + unit.Class);
                    ClassData oldClass = classes[unit.Class];
                    List<ClassData> options = unitClassData.ClassDatas.FindAll(a => a.Flies == oldClass.Flies);
                    unit.Class = options[Random.Range(0, options.Count)].Name;
                    if (unit == boss)
                    {
                        map.ObjectiveData = unit.Class;
                    }
                }
            }
        }
    }
}
