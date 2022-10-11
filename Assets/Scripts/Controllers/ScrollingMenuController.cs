using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingMenuController : MenuController
{
    [Header("Scrolling Menu Controller")]
    public int NumItemsPerPage;
    public float ItemHeight;
    public GameObject Arrows;
    private int currentPage;

    protected override void Start()
    {
        base.Start();
        SetPage(currentPage);
    }

    public override void SelectItem(int index)
    {
        base.SelectItem(index);
        if (index < currentPage)
        {
            SetPage(index);
        }
        else if (index >= currentPage + NumItemsPerPage)
        {
            SetPage(index - NumItemsPerPage + 1);
        }
        if (MenuItems.Count > NumItemsPerPage)
        {
            Arrows.SetActive(true);
        }
    }

    private void SetPage(int page)
    {
        for (int i = 0; i < count; i++)
        {
            // Disable all items below & above it
            MenuItems[i].gameObject.SetActive(i >= page && i < page + NumItemsPerPage);
            // Move all items up/down according to the difference
            MenuItems[i].TheRectTransform.anchoredPosition += new Vector2(0, ItemHeight * (page - currentPage));
        }
        // Update the page
        currentPage = page;
    }
}
