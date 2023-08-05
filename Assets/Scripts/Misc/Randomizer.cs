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
            nameToDisplayName.Add(origin[i].Name, origin[i].TheDisplayName);
        }
        Bugger.Info(string.Join(", ", nameToDisplayName.Keys));
        // Units
        foreach (UnitData unitData in unitClassData.UnitDatas)
        {
            // Always replace fliers with other fliers, and 0-range units with other 0-range units
            ClassData oldClass = classes[unitData.Class];
            List<ClassData> options = unitClassData.ClassDatas.FindAll(a => a.Flies == oldClass.Flies && Mathf.Min(a.Weapon.Range, 1) == Mathf.Min(oldClass.Weapon.Range, 1));
            ClassData newClass = options.RandomItemInList();
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
            if (map.LevelNumber < 1) // It will never get selected, so
            {
                continue;
            }
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
                    //Bugger.Info(map.Name + ": " + unit.Class);
                    ClassData oldClass = classes[unit.Class];
                    List<ClassData> options = unitClassData.ClassDatas.FindAll(a => a.Flies == oldClass.Flies && Mathf.Min(a.Weapon.Range, 1) == Mathf.Min(oldClass.Weapon.Range, 1));
                    unit.Class = options.RandomItemInList().Name;
                    if (unit == boss)
                    {
                        map.ObjectiveData = unit.Class;
                    }
                }
            }
        }
        // Hue-shifting, because why not
        foreach (Tileset tileset in mapController.Tilesets)
        {
            HueShifter shifter = new HueShifter(false);
            tileset.Palette1 = shifter.Apply(tileset.Palette1);
            tileset.Palette2 = shifter.Apply(tileset.Palette2);
        }
        HueShifter metadataShifter = new HueShifter(true);
        for (int i = 0; i < levelMetadataController.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                levelMetadataController[i].TeamDatas[j].Palette = metadataShifter.Apply(levelMetadataController[i].TeamDatas[j].Palette);
            }
        }
    }

    private class HueShifter
    {
        private int shift;
        private bool preventWhite;

        public HueShifter(bool preventWhite)
        {
            this.preventWhite = preventWhite;
            shift = Random.Range(0, CompletePalette.BrightnessJump - (preventWhite ? 2 : 1));
        }

        public Palette Apply(Palette source)
        {
            for (int i = 0; i < 4; i++)
            {
                // White & black will always stay the same
                int origin = source[i];
                if (origin % CompletePalette.BrightnessJump != CompletePalette.BlackColor % CompletePalette.BrightnessJump &&
                    origin != CompletePalette.TransparentColor && (!preventWhite || origin % CompletePalette.BrightnessJump != 0))
                {
                    int newColor = origin % CompletePalette.BrightnessJump + shift;
                    if (origin % CompletePalette.BrightnessJump + shift >= CompletePalette.BlackColor % CompletePalette.BrightnessJump)
                    {
                        newColor -= CompletePalette.BrightnessJump - (preventWhite ? 2 : 1);
                    }
                    origin = (origin / CompletePalette.BrightnessJump) * CompletePalette.BrightnessJump + newColor;// % CompletePalette.BrightnessJump;
                }
                source[i] = origin;
            }
            return source;
        }
    }
}
