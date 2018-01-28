using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hive : ADamageable {

	protected override bool SendDamageAcrossNetwork {
		get {
			return true;
		}
	} 
	public Enemy[] enemies;
	//public int hiveOriginalColor = 1;

	public int numChanges = 4;
	public float hiveMinScale = 1f;
	public float spawnDistanceFromEdge = 10f;
	public float hiveSpawnRate = 1f; 
	float lastSpawn;
	float ogScaleValue;

    // Use this for initialization
    void Start () 
	{
 		ogScaleValue = transform.localScale.x;
	}
	
	// Update is called once per frame
	void Update () 
	{
		HandleSpawns();
	}
	
	public void SetEnemyColor(int id)
	{
		enemyColorId = id;
		GetComponent<Renderer>().material.color = PlayerColor.getColorForId(enemyColorId);
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

	[PunRPC]
	protected override void RemoveDamage(int amount)
	{
		float healthPercentageBefore = (float)currentHealth/(float)startingHealth;
		base.RemoveDamage(amount);
		float healthPercentage = (float)currentHealth/(float)startingHealth;
		float percentageDifference = 1f/numChanges;

		for (float f = percentageDifference; f < healthPercentageBefore; f+= percentageDifference)
		{
			if (healthPercentage <= f)
			{
				SetEnemyColor(enemyColorId == 1 ? 2 : 1);
				break;
			}
		}

		float distanceBetweenScales = ogScaleValue - hiveMinScale;
		float scale = distanceBetweenScales * healthPercentage + hiveMinScale;
		transform.localScale = new Vector3(scale, scale, scale);
	}
	void OnPhotonInstantiate(PhotonMessageInfo info) 
	{
		GameJamGameManager gm = GameJamGameManager.instance;
	    // e.g. store this gameobject as this player's charater in PhotonPlayer.TagObject
	    PhotonView pv = GetComponent<PhotonView>();
		SetEnemyColor((int) pv.instantiationData[0]);
	}
	
}