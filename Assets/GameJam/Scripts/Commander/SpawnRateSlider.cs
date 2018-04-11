using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnRateSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Button spawnNowButton;
    // Use this for initialization
    void Start () {
        slider.onValueChanged.AddListener(SliderValueChanged);
        spawnNowButton.onClick.AddListener(SpawnNow);
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

        slider.transform.parent.gameObject.SetActive(toShow);

        if (toShow)
        {
            valueChangedByAveraging = true;
            slider.value = (average / GenericSelectable.currentlySelected.Count);
        }
    }
    bool valueChangedByAveraging; //Should only display the change in the slider. Only apply it if manually moved and shit.

    void SliderValueChanged(float value)
    {
        text.text = value.ToString();

        if (valueChangedByAveraging)
        {
            valueChangedByAveraging = false;
            return;
        }

        foreach (var s in GenericSelectable.currentlySelected)
        {
            if (s is BaseSelectable)
            {
                var b = s as BaseSelectable;
                b.spawnRate = value;
            }
        }
    }

    void SpawnNow()
    {
        foreach (var s in GenericSelectable.currentlySelected)
        {
            if (s is BaseSelectable)
            {
                var b = s as BaseSelectable;
                b.SpawnNow();
            }
        }
    }
}