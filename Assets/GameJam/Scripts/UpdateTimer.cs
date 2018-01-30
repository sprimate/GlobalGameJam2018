using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTimer : MonoBehaviour {
	Text t;
	float timeStarted;
	void Start () {
		t = GetComponent<Text>();
		timeStarted = Time.time;

	}
	
	// Update is called once per frame
	void Update () {
		t.text = Mathf.CeilToInt(Time.time - timeStarted).ToString();
	}
}
