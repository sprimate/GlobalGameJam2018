using System;
using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;

public class SpeedPickup : APickup {


	protected override void DoPowerUp(Player receiver)
	{
		receiver.TemporarySpeedIncrease();



	}

}