using System;
using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;

public class FiringRatePickup : APickup {


	protected override void DoPowerUp(Player receiver)
	{
		receiver.TemporaryFiringRateIncrease ();



	}

}