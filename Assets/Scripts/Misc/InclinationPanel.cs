using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InclinationPanel : MonoBehaviour
{
    public List<Sprite> Icons;
    public Image Growth1;
    public Image Growth2;
    public Image Bonus;
    public Text Text;
    public void Display(Unit unit)
    {
        if (!KnowledgeController.HasKnowledge(HardcodedKnowledge.InclinationBuff))
        {
            Destroy(gameObject);
        }
        Text.text = unit.Inclination + " inclination.\n" +
            "Increases     and     growths.\n";
        SetIcon(Growth1, (int)unit.Inclination, 0);
        SetIcon(Growth2, (int)unit.Inclination, 1);
        GetComponent<PalettedSprite>().Palette = ((int)unit.Inclination + 1) % 3;
        if (unit.TheTeam == Team.Player)
        {
            Text.text += "Bonus     against " + unit.Inclination.ToString().ToLower() + ".";
            SetIcon(Bonus, (int)unit.Inclination, 0);
        }
        else
        {
            GetComponent<RectTransform>().sizeDelta -= new Vector2(0, 8);
            Bonus.transform.parent.gameObject.SetActive(false);
        }

    }
    private void SetIcon(Image image, int id, int mod)
    {
        image.sprite = Icons[id * 2 + mod];
        PalettedSprite[] palettedSprites = image.transform.parent.GetComponentsInChildren<PalettedSprite>();
        foreach (var item in palettedSprites)
        {
            item.Palette = (id + 1) % 3;
        }
    }
}
