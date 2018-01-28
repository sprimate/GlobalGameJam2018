using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderSpawner : MonoBehaviour {
	public Enemy[] enemies;
	public float spawnDistanceFromEdge = 10f;
	public float spawnRate {protected get; set;}
	float lastSpawn;

	void Start()
	{
		for(int i = 0; i < enemies.Length; i++)
		{
			
			GameObject e = Instantiate(enemies[i].gameObject) as GameObject;
			e.SetActive(false);
			e.transform.SetParent(transform);
			enemies[i] = e.GetComponent<Enemy>();
		}
	}

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
			int index = Random.Range(0, enemies.Length);

			Enemy toSpawn = enemies[index];
			if (toSpawn == null)
			{
				StartCoroutine(KeepTrying(index));
				/*int i = 0;
				foreach(var enemy in enemies)
				{
					i++;
					Debug.Log("Enemy " + i + ": " + enemy, gameObject);
				}
				Debug.Log(toSpawn + " - " + enemies.Length + " - " + index + "-" +  enemies[index], gameObject);*/

			}
			//toSpawn.enemyColorId = Random.Range(1, 3); //3 is exclusive
			PhotonNetwork.InstantiateSceneObject(toSpawn.gameObject.name, spawnPosition, Quaternion.LookRotation(randomDirection), 0, null);				
			lastSpawn = Time.time;
		}
	}

	IEnumerator KeepTrying(int inx)
	{
		int i = 0;
		while(enemies[inx] == null)
		{
			yield return null;
			Debug.Log("Still trying for " + inx + " on " + gameObject.name, gameObject );
			i++;
		}
		Debug.Log("Got it after " + i + "Frames" );
	}
}
