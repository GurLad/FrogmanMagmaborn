using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InclinationIndicator : MonoBehaviour
{
    public List<Sprite> Icons;
    public Image Icon;
    public bool BattlePreview;
    private void Start()
    {
        if (!KnowledgeController.HasKnowledge(HardcodedKnowledge.InclinationBuff))
        {
            //if (BattlePreview && gameObject.activeSelf)
            //{
            //    transform.parent.parent.gameObject.GetComponent<RectTransform>().sizeDelta -= new Vector2(4, 0);
            //}
            Destroy(gameObject);
        }
    }
    public void Display(Unit unit, Unit target)
    {
        if (unit.TheTeam == Team.Player && unit.EffectiveAgainst(target))
        {
            SetIcon(Icon, (int)unit.Inclination, 0);
        }
        else
        {
            SetIcon(Icon, (int)unit.Inclination, 1);
        }

    }
    private void SetIcon(Image image, int id, int mod)
    {
        image.sprite = Icons[id * 2 + mod];
        PalettedSprite[] palettedSprites = image.transform.parent.GetComponentsInChildren<PalettedSprite>();
        foreach (var item in palettedSprites)
        {
            if (mod == 1) // Grayscale indicators when no offensive buff
            {
                item.Palette = 3;
            }
            else
            {
                item.Palette = (id + 1) % 3;
            }
        }
    }
}
