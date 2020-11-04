using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum KnowledgeUpgradeType { Toggle, Choice }
public enum HardcodedKnowledge { LevelUpChoice, InclinationBuff } // For convenience only
public class KnowledgeController : MonoBehaviour
{
    public List<KnowledgeUpgrade> Upgrades;
    public Sprite ActiveSprite;
    public Sprite InactiveSprite;
    [Header("Objects")]
    public MenuController UpgradeMenu;
    public Text Description;
    public Text Amount;
    public Text Cost;
    public KnowledgeMenuItem BaseMenuItem;
    private int knowledge;
    public static bool HasKnowledge(HardcodedKnowledge name)
    {
        return SavedData.Load<int>("KnowledgeUpgrade" + name) == 1;
    }
    public static int GetInclination(string characterName)
    {
        return SavedData.Load<int>("KnowledgeUpgradeInclination" + characterName);
    }
    public static void UnlockKnowledge(string name)
    {
        SavedData.Save("KnowledgeUpgrade" + name, 0);
    }
    public int Knowledge
    {
        get
        {
            return knowledge;
        }
        set
        {
            knowledge = value;
            SavedData.Save("Knowledge", knowledge);
            Amount.text = "Knowledge:" + knowledge.ToString().PadLeft(2);
        }
    }
    private void Awake()
    {
        Knowledge = SavedData.Load<int>("Knowledge");
        for (int i = 0; i < Upgrades.Count; i++)
        {
            Upgrades[i].Load((int)Upgrades[i].DefaultState);
            if (Upgrades[i].State == KnowledgeUpgrade.UpgradeState.Locked)
            {
                continue;
            }
            KnowledgeMenuItem item = Instantiate(BaseMenuItem.gameObject, UpgradeMenu.transform).GetComponent<KnowledgeMenuItem>();
            RectTransform rectTransform = item.GetComponent<RectTransform>();
            rectTransform.SetParent(UpgradeMenu.GetComponent<RectTransform>(), true);
            rectTransform.anchoredPosition += new Vector2(0, -16 * i);
            item.Controller = this;
            item.Upgrade = Upgrades[i];
            item.GetComponent<Text>().text = item.Upgrade.Name.PadRight(12);
            item.gameObject.SetActive(true);
            UpgradeMenu.MenuItems.Add(item);
        }
    }
    public Sprite BuyUpgrade(KnowledgeUpgrade upgrade)
    {
        Knowledge -= upgrade.Cost;
        upgrade.State = KnowledgeUpgrade.UpgradeState.Active;
        return SetUpgradeActive(upgrade, true);
    }
    public Sprite SetUpgradeActive(KnowledgeUpgrade upgrade, bool active)
    {
        upgrade.Active = active;
        upgrade.Save();
        if (active)
        {
            return ActiveSprite;
        }
        else
        {
            return InactiveSprite;
        }
    }
    public int SetUpgradeChoiceValue(KnowledgeUpgrade upgrade, int value)
    {
        if (value <= 0)
        {
            Debug.LogWarning("Using SetUpgradeChoiceValue to set availability is a bad idea");
        }
        upgrade.ChoiceValue = value;
        upgrade.Save();
        return value % 3; // So 1 is physical (red), 2 is technical (blue), 3 = 0 is skillful (green)
    }
}

[System.Serializable]
public class KnowledgeUpgrade
{
    public enum UpgradeState { Available, Active, Inactive, Locked = -1 }
    public string Name;
    public string InternalName;
    public KnowledgeUpgradeType Type;
    public string Description;
    public int Cost;
    public bool Active
    {
        get
        {
            return State == UpgradeState.Active;
        }
        set
        {
            State = value ? UpgradeState.Active : UpgradeState.Inactive;
        }
    }
    public bool Bought
    {
        get
        {
            return State != UpgradeState.Available && State != UpgradeState.Locked;
        }
    }
    public UpgradeState DefaultState;
    public int NumChoices;
    [HideInInspector]
    public int ChoiceValue;
    private UpgradeState state;
    public UpgradeState State
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
            ChoiceValue = (int)state;
        }
    }
    public void Save()
    {
        SavedData.Save("KnowledgeUpgrade" + InternalName, Type == KnowledgeUpgradeType.Toggle ? (int)State : ChoiceValue);
    }
    public void Load(int defaultValue)
    {
        if (Type == KnowledgeUpgradeType.Toggle)
        {
            State = (UpgradeState)SavedData.Load("KnowledgeUpgrade" + InternalName, defaultValue);
        }
        else
        {
            ChoiceValue = SavedData.Load("KnowledgeUpgrade" + InternalName, defaultValue);
            state = (UpgradeState)Mathf.Min(1, ChoiceValue);
        }
    }
}