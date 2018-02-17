using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;

public class Soul : MonoBehaviour {
	public float value {get; set;}
	void OnTriggerEnter (Collider other)
	{
		Player player = other.gameObject.GetComponent<Player>();
		if(player != null)
		{
			player.CollectSoul(value);
			Destroy(gameObject);
		}
	}
}
