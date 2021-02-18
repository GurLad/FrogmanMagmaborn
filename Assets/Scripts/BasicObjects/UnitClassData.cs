using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClassData : MonoBehaviour
{
    public List<UnitClass> UnitClasses;
    public List<GrowthsStruct> UnitGrowths;
    public List<ClassData> ClassDatas;
    public AdvancedSpriteSheetAnimation BaseAnimation;

    #if UNITY_EDITOR
    public void AutoLoad()
    {
        // Load json
        string json = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/Classes.json").text;
        JsonUtility.FromJsonOverwrite("{" + '"' + "ClassDatas" + '"' + ":" + json + "}", this);
        // Load animations
        for (int i = 0; i < ClassDatas.Count; i++)
        {
            ClassDatas[i].MapSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/Images/ClassMapSprites/" + ClassDatas[i].Name + ".png");
        }
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
    #endif
}

[System.Serializable]
public class GrowthsStruct
{
    public string Name;
    [Header("Growths (STR, END, PIR, ARM, PRE, EVA)")]
    public int[] Growths = new int[6];
    public Inclination Inclination;
}

[System.Serializable]
public class ClassData
{
    public string Name;
    public bool Flies;
    public Inclination Inclination;
    public int[] Growths = new int[6];
    public Weapon Weapon;
    public Sprite MapSprite;

    public override string ToString()
    {
        return Name;
    }
}

[System.Serializable]
public class UnitClass
{
    public string Unit;
    public string Class;
}