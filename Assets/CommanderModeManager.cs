using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommanderModeManager : MonoSingleton<CommanderModeManager> {

	public bool commanderMode;
	bool lastCommanderMode;
	[SerializeField] List<GameObject> toToggleInCommanderMode;
	[SerializeField] List<GameObject> toToggleInShipMode;
	// Use this for initialization
	void Awake () {
		UpdateActiveObjects();
	}
	
	
	// Update is called once per frame
	void Update () {
		if (lastCommanderMode != commanderMode)
		{
			UpdateActiveObjects();
		}
		lastCommanderMode = commanderMode;

		if (Input.GetKeyUp(KeyCode.C))
		{
			commanderMode = !commanderMode;
		}
	}

	void UpdateActiveObjects()
	{
		foreach(var g in toToggleInCommanderMode)
		{
			g.SetActive(commanderMode);
		}
		foreach(var g in toToggleInShipMode)
		{
			g.SetActive(!commanderMode);
		}
	}
}