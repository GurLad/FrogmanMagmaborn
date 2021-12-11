using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClassData : MonoBehaviour
{
    public List<UnitData> UnitDatas;
    public List<ClassData> ClassDatas;
    public AdvancedSpriteSheetAnimation BaseAnimation;

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        // Load classes json
        string json = FrogForgeImporter.LoadTextFile("Classes.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("ClassDatas"), this);
        // Load classes animations
        for (int i = 0; i < ClassDatas.Count; i++)
        {
            ClassDatas[i].MapSprite = FrogForgeImporter.LoadSpriteFile("Images/ClassMapSprites/" + ClassDatas[i].Name + ".png");
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        // Load units json
        json = FrogForgeImporter.LoadTextFile("Units.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("UnitDatas"), this);
    }
#endif

    [System.Serializable]
    public class GrowthsStruct
    {
        [Header("Growths (STR, END, PIR, ARM, PRE, EVA)")]
        public int[] Values = new int[6];
    }
}

[System.Serializable]
public class UnitData
{
    public string Name;
    public string DisplayName;
    public string Class;
    public Inclination Inclination;
    public UnitClassData.GrowthsStruct Growths;
    [TextArea]
    public string DeathQuote;
}

[System.Serializable]
public class ClassData
{
    public string Name;
    public bool Flies;
    public Inclination Inclination;
    public UnitClassData.GrowthsStruct Growths;
    public Weapon Weapon;
    public Sprite MapSprite;

    public override string ToString()
    {
        return Name;
    }
}