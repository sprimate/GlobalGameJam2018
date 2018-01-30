using System;
using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;

public class HealthPickup : APickup {

	public int healthAmount = 40;

	protected override void DoPowerUp(Player receiver)
	{
		receiver.currentHealth += healthAmount;
		if (receiver.startingHealth > receiver.currentHealth)
		{
			receiver.currentHealth = receiver.startingHealth;
		}
	}

}