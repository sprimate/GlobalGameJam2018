using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AContextMenuButton : Button {

	protected override void Start()
	{
		base.Start();
		onClick.AddListener(Action);
	}
	public abstract void Action();
}
