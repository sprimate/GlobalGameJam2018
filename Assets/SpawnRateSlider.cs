using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnRateSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI text;
    // Use this for initialization
    void Start () {
        slider.onValueChanged.AddListener(SliderValueChanged);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        bool toShow = true && GenericSelectable.currentlySelected.Count > 0;
        float average = 0f;
        foreach (var selected in GenericSelectable.currentlySelected)
        {
            if (!(selected is BaseSelectable))
            {
                toShow = false;
                break;
            }
            var b = selected as BaseSelectable;
            average += b.spawnRate;
        }

        slider.gameObject.SetActive(toShow);

        if (toShow)
        {
            slider.value = (average / GenericSelectable.currentlySelected.Count);
        }
    }

    void SliderValueChanged(float value)
    {
        foreach (var s in GenericSelectable.currentlySelected)
        {
            if (s is BaseSelectable)
            {
                var b = s as BaseSelectable;
                b.spawnRate = Mathf.RoundToInt(value);
            }
        }
        text.text = Mathf.RoundToInt(value).ToString();
    }
}