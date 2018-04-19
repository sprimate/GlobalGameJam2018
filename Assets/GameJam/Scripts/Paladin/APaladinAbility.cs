using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class APaladinAbility : MonoBehaviour {
    public bool inactiveByDefault;
    public KeyCode hotkey;
    public int playerId;
    public int abilityNum;
	public float cooldown;
	public string abilityName;
	public string description;
	public Sprite icon;
	protected float lastUsedTime;
    protected AbilityButton buttonInterface;

	public bool CanUse
	{
		get {
			return Time.time - cooldown >= lastUsedTime;
		}
	}
	
	public virtual void Activate()
	{
		if (CanUse)
		{
			Use();
		}
	}
    protected virtual void Use()
    {
        Confirm();
    }


	protected virtual void Confirm()
	{
        lastUsedTime = Time.time;
        StartCoroutine(UpdateCoutdown());
	}

    IEnumerator UpdateCoutdown()
    {
        while (buttonInterface == null)
        {
            yield return null;
        }
        while (!CanUse)
        {
            if (buttonInterface != null)
            {
                buttonInterface.currentCooldown.text = Mathf.CeilToInt(cooldown - (Time.time - lastUsedTime)).ToString();
                yield return new WaitForSeconds(1f);
            }
        }
        buttonInterface.currentCooldown.text = "";
    }
	
	protected virtual void Update()
	{
        if (buttonInterface != null)
        {
            buttonInterface.SetInteractable(CanUse);
        }

        if (Input.GetKeyUp(hotkey))
        {
            Activate();
        }
	}

    protected virtual void UpdateAbilityDescription()
    {
        AbilityDescription.instance.description.text = description.ToString();
        AbilityDescription.instance.cooldownText.text = cooldown.ToString();
        AbilityDescription.instance.damageText.text = "?";
    }

    void Awake()
	{
		lastUsedTime = inactiveByDefault ? 0f :  -cooldown;
	}

    IEnumerator Start()
    {
        while (buttonInterface == null)
        {
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        if (inactiveByDefault)
        {
            lastUsedTime = Time.time;
            StartCoroutine(UpdateCoutdown());
        }
    }

    public void SetInterface(AbilityButton button)
    {
        buttonInterface = button;
        buttonInterface.abilityName.text = abilityName;
        buttonInterface.AddSelectionListeners(Activate, UpdateAbilityDescription);
    }
}
