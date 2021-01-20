using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClassData : MonoBehaviour
{
    public List<UnitClass> UnitClasses;
    public List<GrowthsStruct> UnitGrowths;
    public List<ClassData> ClassDatas;
    public List<ClassAnimation> ClassAnimations;

    #if UNITY_EDITOR
    public void AutoLoad()
    {
        //Debug.Log(JsonUtility.ToJson(ClassDatas));
        string json = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/Classes.json").text;
        Debug.Log(json);
        JsonUtility.FromJsonOverwrite("{" + '"' + "ClassDatas" + '"' + ":" + json + "}", this);
        // this = JsonUtility.FromJson<UnitClassData>("{" + '"' + "ClassDatas" + '"' + ":" + json + "}");
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