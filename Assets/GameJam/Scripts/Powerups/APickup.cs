using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CompleteProject;

[RequireComponent (typeof(PhotonView))]
public abstract class APickup : Photon.MonoBehaviour{

	protected virtual void Awake()
	{

	}

	void OnTriggerEnter(Collider other) {
		Player receiver = other.gameObject.GetComponent<Player> ();

//		Debug.Log ("pickup trigger enter. rx:"+receiver);

		if (receiver) {
//			Debug.Log ("doing powerup");
			DoPowerUp (receiver);
			Destroy (gameObject);

		}
	}


	protected virtual void DoPowerUp(Player receiver)
	{

	}


}