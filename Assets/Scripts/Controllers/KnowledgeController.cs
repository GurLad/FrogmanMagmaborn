using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnowledgeController : MonoBehaviour
{
    public enum UpgradeState { Available, Active, Inactive, Locked = -1 }
    public List<KnowledgeUpgradeList> UpgradeMenus;
    public Sprite ActiveSprite;
    public Sprite InactiveSprite;
    public Sprite[] InclinationSprites;
    [Header("Objects")]
    public MenuController BaseMenu;
    public Text MenuName;
    public Text Description;
    public Text Amount;
    public Text Cost;
    public GameObject BaseMenuItem;
    public GameObject Arrows;
    private List<MenuController> menus = new List<MenuController>();
    private int selectedMenu;
    private int knowledge;
    private bool pressedLastFrame;
    public static bool HasKnowledge(HardcodedKnowledge name)
    {
        return SavedData.Load<int>("Knowledge", "Upgrade" + name) == 1;
    }
    public static bool HasKnowledge(string name)
    {
        return SavedData.Load<int>("Knowledge", "Upgrade" + name) == 1;
    }
    public static int GetInclination(string characterName)
    {
        return SavedData.Load<int>("Knowledge", "UpgradeInclination" + characterName);
    }
    public static void UnlockKnowledge(string name)
    {
        SavedData.Save("Knowledge", "Upgrade" + name, 0);
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
            SavedData.Save("Knowledge", "Amount", knowledge);
            Amount.text = "Knowledge:" + knowledge.ToString().PadLeft(2);
        }
    }
    private void Awake()
    {
        Knowledge = SavedData.Load<int>("Knowledge", "Amount");
        for (int i = 0; i < UpgradeMenus.Count; i++) // For each menu
        {
            int pos = 0;
            // Create a new menu
            MenuController menu = Instantiate(BaseMenu.gameObject, BaseMenu.transform.parent).GetComponent<MenuController>();
            menus.Add(menu);
            menu.gameObject.SetActive(i == 0);
            // Add upgrades
            foreach (KnowledgeUpgrade upgrade in UpgradeMenus[i].Upgrades)
            {
                upgrade.Load((int)upgrade.DefaultState);
                if (upgrade.State == UpgradeState.Locked)
                {
                    continue;
                }
                KnowledgeMenuItem item;
                switch (UpgradeMenus[i].Type)
                {
                    case KnowledgeUpgradeType.Toggle:
                        item = Instantiate(BaseMenuItem, menu.transform).AddComponent<ToggleKnowledgeMenuItem>();
                        break;
                    case KnowledgeUpgradeType.Inclination:
                        item = Instantiate(BaseMenuItem, menu.transform).AddComponent<InclinationKnowledgeMenuItem>();
                        break;
                    default:
                        throw new System.Exception("No type?");
                }
                // A very bad code for finding the indicators
                item.Indicators = new List<GameObject>();
                foreach (Transform child in item.transform)
                {
                    if (child.name.Contains("State"))
                    {
                        item.BoughtIndicator = child.gameObject.GetComponent<Image>();
                    }
                    else
                    {
                        item.Indicators.Add(child.gameObject);
                    }
                }
                RectTransform rectTransform = item.GetComponent<RectTransform>();
                rectTransform.SetParent(menu.GetComponent<RectTransform>(), true);
                rectTransform.anchoredPosition += new Vector2(0, -16 * pos++);
                item.Controller = this;
                item.Upgrade = upgrade;
                item.GetComponent<Text>().text = item.Upgrade.Name.PadRight(12);
                item.gameObject.SetActive(true);
                menu.MenuItems.Add(item);
            }
            // Remove the menu if empty (locked)
            if (menu.MenuItems.Count <= 1)
            {
                menus.Remove(menu);
                Destroy(menu.gameObject);
                UpgradeMenus.RemoveAt(i--);
            }
        }
        if (menus.Count < 2) // Only one menu
        {
            Arrows.gameObject.SetActive(false);
        }
        selectedMenu = 0;
    }
    private void Update()
    {
        if (menus.Count < 2)
        {
            return;
        }
        if (Control.GetAxisInt(Control.Axis.X) != 0)
        {
            if (!pressedLastFrame)
            {
                menus[selectedMenu].gameObject.SetActive(false);
                selectedMenu += menus.Count + Control.GetAxisInt(Control.Axis.X);
                selectedMenu %= menus.Count;
                menus[selectedMenu].gameObject.SetActive(true);
                menus[selectedMenu].SelectItem(1);
                MenuName.text = UpgradeMenus[selectedMenu].Name;
                pressedLastFrame = true;
            }
        }
        else
        {
            pressedLastFrame = false;
        }
    }
    public Sprite BuyUpgrade(KnowledgeUpgrade upgrade)
    {
        Knowledge -= upgrade.Cost;
        upgrade.State = UpgradeState.Active;
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

    [System.Serializable]
    public class KnowledgeUpgradeList
    {
        public string Name;
        public KnowledgeUpgradeType Type;
        public List<KnowledgeUpgrade> Upgrades;
    }

    [System.Serializable]
    public class KnowledgeUpgrade
    {
        public string Name;
        public string InternalName;
        [TextArea]
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
        private KnowledgeUpgradeType Type;
        public void Save()
        {
            SavedData.Save("Knowledge", "Upgrade" + InternalName, Type == KnowledgeUpgradeType.Toggle ? (int)State : ChoiceValue);
        }
        public void Load(int defaultValue)
        {
            if (Type == KnowledgeUpgradeType.Toggle)
            {
                State = (UpgradeState)SavedData.Load("Knowledge", "Upgrade" + InternalName, defaultValue);
            }
            else
            {
                ChoiceValue = SavedData.Load("Knowledge", "Upgrade" + InternalName, defaultValue);
                state = (UpgradeState)Mathf.Min(1, ChoiceValue);
            }
        }
    }
}