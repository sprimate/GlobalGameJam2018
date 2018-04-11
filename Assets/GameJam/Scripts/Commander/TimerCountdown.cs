using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerCountdown : MonoBehaviour {

    TextMeshProUGUI text;
	// Use this for initialization
	void Start () {
        text = GetComponent<TextMeshProUGUI>();
	}
	
	// Update is called once per frame
	void Update () {
        if (GameJamGameManager.instance.startTime > 0)
        {
            text.text = (GameJamGameManager.instance.totalGameTime - Mathf.CeilToInt(Time.time - GameJamGameManager.instance.startTime)).ToString();
        }
	}
}
