using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClassData : MonoBehaviour
{
    public List<UnitData> UnitDatas;
    public List<ClassData> ClassDatas;
    public AdvancedSpriteSheetAnimation BaseAnimation;

#if UNITY_EDITOR
    public void AutoLoad()
    {
        // Load classes json
        string json = FrogForgeImporter.LoadFile<TextAsset>("Classes.json").text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("ClassDatas"), this);
        // Load classes animations
        for (int i = 0; i < ClassDatas.Count; i++)
        {
            ClassDatas[i].MapSprite = FrogForgeImporter.LoadFile<Sprite>("Images/ClassMapSprites/" + ClassDatas[i].Name + ".png");
        }
        UnityEditor.EditorUtility.SetDirty(gameObject);
        // Load units json
        json = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/Units.json").text;
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