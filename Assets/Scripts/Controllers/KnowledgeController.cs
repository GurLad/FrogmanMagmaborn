using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum KnowledgeUpgradeType { Toggle, Choice }
public class KnowledgeController : MonoBehaviour
{
    public List<KnowledgeUpgrade> Upgrades;
    public Sprite ActiveSprite;
    public Sprite InactiveSprite;
    [Header("Objects")]
    public MenuController UpgradeMenu;
    public Text Description;
    public KnowledgeMenuItem BaseMenuItem;
    [HideInInspector]
    public int Knowledge;
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
            item.GetComponent<Text>().text = item.Upgrade.Name;
            item.gameObject.SetActive(true);
            UpgradeMenu.MenuItems.Add(item);
        }
    }
    public Sprite BuyUpgrade(KnowledgeUpgrade upgrade)
    {
        Knowledge -= upgrade.Cost;
        SavedData.Save("Knowledge", Knowledge);
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
            return State != UpgradeState.Available && State != KnowledgeUpgrade.UpgradeState.Locked;
        }
    }
    public UpgradeState DefaultState;
    [HideInInspector]
    public UpgradeState State;
    public void Save()
    {
        SavedData.Save("KnowledgeUpgrade" + Name, (int)State);
    }
    public void Load(int defaultValue)
    {
        State = (UpgradeState)SavedData.Load("KnowledgeUpgrade" + Name, defaultValue);
    }
}