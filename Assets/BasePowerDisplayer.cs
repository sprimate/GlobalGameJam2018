using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasePowerDisplayer : MonoSingleton<BasePowerDisplayer> {

	Dictionary<BaseSelectable, TextMeshProUGUI> textToBaseDict = new Dictionary<BaseSelectable, TextMeshProUGUI>();
	public void DisplayPower(BaseSelectable baseSelectable)
	{
		if (!textToBaseDict.ContainsKey(baseSelectable))
		{
			GameObject g = new GameObject();
			g.transform.SetParent(transform);
			textToBaseDict[baseSelectable] = g.AddComponent<TextMeshProUGUI>();
			textToBaseDict[baseSelectable].color = Color.black;
			textToBaseDict[baseSelectable].alignment = TextAlignmentOptions.Center;
		}
		
		Vector3 screenPos = Camera.main.WorldToScreenPoint(baseSelectable.transform.TransformPoint(baseSelectable.GetComponent<SphereCollider>().center));
		textToBaseDict[baseSelectable].transform.position = screenPos;	
		textToBaseDict[baseSelectable].text = baseSelectable.power.ToString();
		
	}
}
