using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommanderModeManager : MonoSingleton<CommanderModeManager> {

    public enum TouchMode { Select, Pan }
    public TouchMode touchMode;
    public Button changeTouchModeButton;
	public bool commanderMode;
	bool lastCommanderMode;
	[SerializeField] List<GameObject> toToggleInCommanderMode;
	[SerializeField] List<GameObject> toToggleInShipMode;
	// Use this for initialization
	void Awake () {
		UpdateActiveObjects();
        changeTouchModeButton.onClick.AddListener(ToggleTouchMode);
	}

    void ToggleTouchMode()
    {
        if (touchMode == TouchMode.Pan)
        {
            touchMode = TouchMode.Select;
            changeTouchModeButton.GetComponentInChildren<Text>().text = "Change to Pan";
        }
        else
        {
            touchMode = TouchMode.Pan;
            changeTouchModeButton.GetComponentInChildren<Text>().text = "Change to Select";
        }
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

    void ChangeTouchMode()
    {

    }
}