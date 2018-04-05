using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class APaladinAbility : MonoBehaviour {
	public float cooldown;
	public string abilityName;
	public string description;
	public Sprite icon;
	public Button button;
	protected float lastUsedTime;

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
	protected abstract void Use();
	protected virtual void Conirm()
	{
		lastUsedTime = Time.time;
	}
	
	void Update()
	{
		button.interactable = CanUse;
	}

	void Awake()
	{
		lastUsedTime -= cooldown;
	}
}
