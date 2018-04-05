using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialAbility : MonoBehaviour {
	public float cooldown;
	public string abilityName;
	public string description;
	public Sprite image;
	
	public abstract void Action();
}
