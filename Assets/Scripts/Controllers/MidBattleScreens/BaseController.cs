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
    public StatusMenuItem StatusMenuItem; // TBA: Replace with the inheriting StatusMenuItem class
    [Header("Objects")]
    public ConversationController BaseConversations;

    public void Show(List<Unit> players)
    {
        // Show the BaseMenu and store the players for the StatusMenu
        BaseMenu.Begin();
        // Populate the talk menu
        List<ConversationData> conversations = BaseConversations.GetAllOptions();
        foreach (ConversationData conversation in conversations)
        {
            MenuItem conversationMenuItem = Instantiate(TalkMenuItem, TalkMenuItem.transform.parent);
            TPlayConversation conversationTrigger = conversationMenuItem.GetComponent<TPlayConversation>();
            conversationTrigger.Data = string.Join("\n", conversation.Lines);
            conversationTrigger.Data += "\n:showBase:\n"; // Show the base again after the conversation is done
            conversationMenuItem.Text = " " + conversation.ToString();
            conversationMenuItem.gameObject.SetActive(true);
            TalkMenu.MenuItems.Add(conversationMenuItem);
        }
        // Populate the status menu
        if (players.Count < 1)
        {
            throw Bugger.Error("Showing base with zero units!");
        }
        List<List<Unit>> unitLists = new List<List<Unit>>();
        for (int i = 0; i < 3; i++)
        {
            if (i == (int)players[0].TheTeam)
            {
                unitLists.Add(players);
            }
            else
            {
                unitLists.Add(new List<Unit>());
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            StatusMenuItem statusMenuItem = Instantiate(StatusMenuItem, StatusMenuItem.transform.parent);
            statusMenuItem.Init(players[i], unitLists); // Show the status of the player & save the list for detailed scrolling
            statusMenuItem.Menu = StatusMenu;
            statusMenuItem.gameObject.SetActive(true);
            statusMenuItem.RectTransform.anchoredPosition = new Vector2(0, -statusMenuItem.RectTransform.sizeDelta.y * i);
            StatusMenu.MenuItems.Add(statusMenuItem);
        }
    }
}
