using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSomethingButton : AContextMenuButton
{
	public GenericSelectable objectToBuild;
    public override void Action()
    {
		foreach(var selectable in GenericSelectable.currentlySelected)
        {
            BaseSelectable s = selectable as BaseSelectable;
            s.PlaceBuildable(objectToBuild);
            ContextMenuManager.instance.CloseMenu();
            return;
        }
    }
}