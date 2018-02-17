using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateNumPlayers : MonoBehaviour {


    public void Awake()
    {
        GameJamGameManager.maxNumPlayersOverride = GetComponent<Toggle>().isOn ? 2 : 1;
    }
    public void OnValueChanged(bool val)
    {
        GameJamGameManager.maxNumPlayersOverride = val ? 2 : 1;
    }
}
