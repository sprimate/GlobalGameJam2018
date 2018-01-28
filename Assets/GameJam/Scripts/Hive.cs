using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hive : ADamageable {
	public Enemy[] enemies;
	public int hiveOriginalColor = 1;

	public int numChanges = 4;
	public float hiveMinScale = 1f;
	public float hiveMaxScale = 1f;
	public float spawnDistanceFromEdge = 10f;
	public float hiveSpawnRate = 1f; 
	float lastSpawn;

    // Use this for initialization
    void Start () {
 		
	}
	
	// Update is called once per frame
	void Update () {
		HandleSpawns();
	}

	public override void TakeDamage(int playerColor, int amount, Vector3 hitPoint)
	{
		base.TakeDamage(playerColor, amount, hitPoint);
	} 

	void HandleSpawns()
	{
		if (!PhotonNetwork.player.IsMasterClient)
		{
			return;
		}
		
		if (lastSpawn + hiveSpawnRate < Time.time)
		{
			var radius = GetComponent<Renderer>().bounds.extents.magnitude;
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
	[PunRPC]
    protected override void Death()
    {
		Debug.Log("HIVE SHOULD BE DEAD");
		Destroy();
    }
}