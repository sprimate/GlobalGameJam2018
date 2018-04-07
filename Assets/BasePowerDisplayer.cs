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
            g.layer = gameObject.layer;
			g.name = baseSelectable.gameObject.name + " (Power)";
			textToBaseDict[baseSelectable] = g.AddComponent<TextMeshProUGUI>();
			textToBaseDict[baseSelectable].color = Color.black;
			textToBaseDict[baseSelectable].alignment = TextAlignmentOptions.Center;
			textToBaseDict[baseSelectable].fontSize = 28f;
		}
		
		Vector3 screenPos = Camera.main.WorldToScreenPoint(baseSelectable.transform.TransformPoint(baseSelectable.GetComponent<SphereCollider>().center));
		textToBaseDict[baseSelectable].transform.position = screenPos;	
		textToBaseDict[baseSelectable].text = Mathf.CeilToInt(baseSelectable.power).ToString();
	}

    public void BaseDestroyed(BaseSelectable selectable)
    {
        Destroy(textToBaseDict[selectable]);
        textToBaseDict.Remove(selectable);
    }
}
