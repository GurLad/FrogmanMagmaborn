using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    // The base controller isn't a MidBattleScreen in itself - instead, it constantly switches between menues (MidBattleScreens)
    [Header("Menus")]
    public MenuController BaseMenu;
    public MenuController TalkMenu;
    public MenuController StatusMenu; // Might seperate it into a different class
    [Header("Menu Items")]
    public MenuItem TalkMenuItem;
    public MenuItem StatusMenuItem; // TBA: Replace with the inheriting StatusMenuItem class
    [Header("Objects")]
    public ConversationController BaseConversations;

    public void Show(List<Unit> players)
    {
        // Show the BaseMenu and store the players for the StatusMenu
        BaseMenu.Begin();
        // Populate the talk menu - TBA: allow for scrolling etc.
        List<ConversationData> conversations = BaseConversations.GetAllOptions();
        foreach (ConversationData conversation in conversations)
        {
            MenuItem conversationMenuItem = Instantiate(TalkMenuItem, TalkMenuItem.transform.parent);
            conversationMenuItem.Text = conversation.ToString();
            conversationMenuItem.gameObject.SetActive(true);
            TalkMenu.MenuItems.Add(conversationMenuItem);
        }
        // Populate the status menu
        foreach (Unit player in players)
        {
            MenuItem statusMenuItem = Instantiate(StatusMenuItem, StatusMenuItem.transform.parent);
            // statusMenuItem.Init(player, players) // Show the status of the player & save the list for detailed scrolling
            statusMenuItem.gameObject.SetActive(true);
            StatusMenu.MenuItems.Add(statusMenuItem);
        }
    }
}
