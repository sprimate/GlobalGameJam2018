using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenuManager : MonoSingleton<ContextMenuManager> {

    Dictionary<ContextMenuButton, int> activeButtons = new Dictionary<ContextMenuButton, int>();
    int maxNum = 0;
    bool contextMenuBuildAlreadyTriggered;
    public void AddToCurrentContextMenu(params MenuButtonContainer[] menus)
    {
        foreach (var menu in menus)
        {
            foreach (var button in menu.menuButtons)
            {
                if (activeButtons.ContainsKey(button))
                {
                    activeButtons[button]++;
                }
                else
                {
                    activeButtons[button] = 1;
                }
                if (activeButtons[button] > maxNum)
                {
                    maxNum = activeButtons[button];
                }
            }
        }
        if (!contextMenuBuildAlreadyTriggered)
        {
            StartCoroutine(BuildContextMenu());
        }
    }

    IEnumerator BuildContextMenu()
    {
        contextMenuBuildAlreadyTriggered = true;
        yield return null;
        List<ContextMenuButton> buttons = new List<ContextMenuButton>();
        foreach (var pair in activeButtons)
        {
            if (pair.Value == maxNum)
            {
                buttons.Add(pair.Key);
            }
        }

        foreach (var b in buttons)
        {
            Debug.Log("BUtton: " + b);
        }
        maxNum = 0;
        activeButtons.Clear();
        contextMenuBuildAlreadyTriggered = false;
    }
}
