using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildSomethingButton : AContextMenuButton
{
	public GenericSelectable objectToBuild;
    public override void Action()
    {
        foreach (var selectable in GenericSelectable.currentlySelected)
        {
            BaseSelectable s = selectable as BaseSelectable;
            s.PlaceBuildable(objectToBuild);
            ContextMenuManager.instance.CloseMenu();
            return;
        }
    }

    public void Start()
    {
        GetComponentInChildren<Text>().text += " (" + objectToBuild.PowerCost + ")";
    }
}