using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour, IPointerUpHandler
{
    public Text currentCooldown;
    public Text abilityName;
    [SerializeField] Text abilityHotkey;
    APaladinAbility ability;
    Action onSelect;
    Action onUpdateInfo;
	// Use this for initialization
	void Start () {
        //GetComponent<Button>().onClick.AddListener(OnClick);
	}

    void OnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            onSelect.Invoke();
        }
        else if (AbilityDescription.instance.gameObject.activeInHierarchy)
        {
            AbilityDescription.instance.gameObject.SetActive(false);
        }
        else
        {
            onUpdateInfo.Invoke();
            AbilityDescription.instance.gameObject.SetActive(true);
        }
    }

    public void AddSelectionListeners(Action _onSelect, Action updateInfo)
    {
        onSelect = _onSelect;
        onUpdateInfo = updateInfo;
    }

    public void SetInteractable(bool val)
    {
        GetComponent<Button>().interactable = val;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Input.GetMouseButtonUp(0))
        {
            onSelect.Invoke();
        }
        else if (AbilityDescription.instance.transform.GetChild(0).gameObject.activeInHierarchy)
        {
            AbilityDescription.instance.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            onUpdateInfo.Invoke();
            AbilityDescription.instance.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
