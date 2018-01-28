using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderSpawner : MonoBehaviour {
	public Enemy[] enemies;
	public float spawnDistanceFromEdge = 10f;
	public float spawnRate {protected get; set;}
	float lastSpawn;

	protected virtual void Update () 
	{
		HandleSpawns();
	}
	void HandleSpawns()
	{
		if (!PhotonNetwork.player.IsMasterClient)
		{
			return;
		}
		
		if (lastSpawn + spawnRate < Time.time)
		{
			var radius = GetComponent<SphereCollider>().radius * transform.localScale.x;//GetComponent<Renderer>().bounds.extents.magnitude;
			//Debug.Log("Radius: " + radius);
			var spawnDistance = radius + spawnDistanceFromEdge;
			var randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f,1f));
			Vector3 spawnPosition = transform.position + (randomDirection * spawnDistance);
			Enemy toSpawn = enemies[Random.Range(0, enemies.Length)];
			//toSpawn.enemyColorId = Random.Range(1, 3); //3 is exclusive
			PhotonNetwork.InstantiateSceneObject(toSpawn.gameObject.name, spawnPosition, Quaternion.LookRotation(randomDirection), 0, null);				
			lastSpawn = Time.time;
		}
	}
}
