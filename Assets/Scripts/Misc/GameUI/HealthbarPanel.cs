using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthbarPanel : MonoBehaviour
{
    private const int TICK_SIZE = 4;
    [SerializeField]
    private int maxValue;
    [SerializeField]
    private RectTransform full;
    [SerializeField]
    private RectTransform value;
    [SerializeField]
    private RectTransform container;
    private List<RectTransform> values = new List<RectTransform>();
    private int count;

    private void Reset()
    {
        container = GetComponent<RectTransform>();
    }

    public void SetMax(int maxHealth)
    {
        count = (maxHealth - 1) / maxValue + 1;
        container.sizeDelta = new Vector2(container.sizeDelta.x, container.sizeDelta.y + value.sizeDelta.y * (count - 1));
        for (int i = 0; i < count; i++)
        {
            RectTransform newFull = Instantiate(full.gameObject, container).GetComponent<RectTransform>();
            newFull.anchoredPosition = new Vector2(newFull.anchoredPosition.x, newFull.anchoredPosition.y + i * full.sizeDelta.y);
            newFull.sizeDelta = new Vector2(TICK_SIZE * HealthToWidth(maxHealth, i), newFull.sizeDelta.y);
            RectTransform newValue = Instantiate(value.gameObject, container).GetComponent<RectTransform>();
            newValue.anchoredPosition = new Vector2(newValue.anchoredPosition.x, newValue.anchoredPosition.y + i * value.sizeDelta.y);
            values.Add(newValue);
        }
        SetValue(maxHealth);
        Destroy(full.gameObject);
        Destroy(value.gameObject);
    }

    public void SetValue(int health)
    {
        for (int i = 0; i < count; i++)
        {
            int width = HealthToWidth(health, i);
            if (width > 0)
            {
                values[i].sizeDelta = new Vector2(TICK_SIZE * HealthToWidth(health, i), values[i].sizeDelta.y);
            }
            else
            {
                Destroy(values[i].gameObject);
                values.RemoveAt(i--);
                count--;
            }
        }
    }

    private int HealthToWidth(int health, int barIndex)
    {
        return Mathf.Min(health - maxValue * barIndex, maxValue);
    }
}
