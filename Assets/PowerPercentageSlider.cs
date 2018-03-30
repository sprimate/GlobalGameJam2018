using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerPercentageSlider : MonoSingleton<PowerPercentageSlider> {

	float _powerPercentage;
    public float powerPercentageOnDrag {
		get
		{
			return _powerPercentage;
		}
		set
		{
			_powerPercentage = value;
			text.text = Mathf.CeilToInt(_powerPercentage).ToString();
		}
	}
	Slider slider;
	[SerializeField] TextMeshProUGUI text;
	// Use this for initialization
	void Start () {
		powerPercentageOnDrag = 50f;
		slider = GetComponent<Slider>();
		slider.value = powerPercentageOnDrag / 100f;
		slider.onValueChanged.AddListener(SliderUpdated);
	}
	
	void SliderUpdated(float value)
	{
		powerPercentageOnDrag = value * 100f;
	}
}
