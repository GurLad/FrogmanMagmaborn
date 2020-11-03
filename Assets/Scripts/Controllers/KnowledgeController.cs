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
}

[System.Serializable]
public class KnowledgeUpgrade
{
    public enum UpgradeState { Available, Active, Inactive, Locked }
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
    [HideInInspector]
    public UpgradeState State;
    public void Save()
    {
        SavedData.Save("KnowledgeUpgrade" + InternalName, (int)State);
    }
    public void Load(int defaultValue)
    {
        State = (UpgradeState)SavedData.Load("KnowledgeUpgrade" + InternalName, defaultValue);
    }
}