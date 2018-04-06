using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuManager : MonoSingleton<ContextMenuManager> {

    Dictionary<AContextMenuButton, int> activeButtons = new Dictionary<AContextMenuButton, int>();
    int maxNum = 0;
    bool contextMenuBuildAlreadyTriggered;
    public void AddToCurrentContextMenu(params MenuButtonContainer[] menus)
    {
        Debug.Log("How i get here");
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

    HorizontalLayoutGroup horizontalLayoutGroup;

    Coroutine nextFrameCoroutine;
    public void AddMenu(IEnumerable<AContextMenuButton> buttons, bool duplicate = true)
    {
        if (nextFrameCoroutine == null)
        {
            nextFrameCoroutine = StartCoroutine(NextFrame(buttons, duplicate));
        }
        else
        {
            StartCoroutine(CancelMenuCoroutine());
        }
    }

    IEnumerator CancelMenuCoroutine()
    {

        yield return new WaitForEndOfFrame();
        if (nextFrameCoroutine == null)
        {
            yield break;
        }
        StopCoroutine(nextFrameCoroutine);
        nextFrameCoroutine = null;
    }

    IEnumerator NextFrame(IEnumerable<AContextMenuButton> buttons, bool duplicate)
    {
        if (horizontalLayoutGroup == null)
        {
            GameObject g = new GameObject("Menu");
            horizontalLayoutGroup = g.AddComponent<HorizontalLayoutGroup>();
            g.transform.SetParent(transform, false);
        }
        GameObject newGroup = new GameObject("Vertical Layout Group");
        var vertLayoutGroup = newGroup.AddComponent<VerticalLayoutGroup>();
        vertLayoutGroup.transform.SetParent(horizontalLayoutGroup.transform);
        yield return null;
        foreach(var b in buttons)
        {
            var button = b;
            if (duplicate)
            {
                button = Instantiate(b);
            }
            button.transform.SetParent(vertLayoutGroup.transform);
            nextFrameCoroutine = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        if (horizontalLayoutGroup != null)
        {
            Destroy(horizontalLayoutGroup.gameObject);
            horizontalLayoutGroup = null;
        }
    }

    IEnumerator BuildContextMenu()
    {
        contextMenuBuildAlreadyTriggered = true;
        yield return null;
        List<AContextMenuButton> buttons = new List<AContextMenuButton>();
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
