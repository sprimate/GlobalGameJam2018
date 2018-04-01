using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OpenBuildMenuButton : AContextMenuButton {
	public List<AContextMenuButton> buttons = new List<AContextMenuButton>();
	public override void Action()
	{
		/*List<AContextMenuButton> buttons = new List<AContextMenuButton>();
		foreach(var b in buildables)
		{
			GameObject newButton = Instantiate(gameObject);
			newButton.transform.name = "Build " + b.transform.name;
			DestroyImmediate(newButton.GetComponent<OpenBuildMenuButton>());
			BuildSomethingButton button = newButton.AddComponent<BuildSomethingButton>();
			button.objectToBuild = b;
			buttons.Add(button);
		}*/
		ContextMenuManager.instance.AddMenu(buttons);
	}
}