using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hive : ADamageable {

	protected override bool SendDamageAcrossNetwork {
		get {
			return true;
		}
	} 
	//public int hiveOriginalColor = 1;

	public int numChanges = 4;
	public float hiveMinScale = 1f;
	public float hiveMinSpawnRate = 1f; 
	public float hiveMaxSpawnRate = 4f;
	float lastSpawn;
	float ogScaleValue;
	BorderSpawner borderSpawner;

    // Use this for initialization
    void Start () 
	{
		borderSpawner = GetComponent<BorderSpawner>();
 		ogScaleValue = transform.localScale.x;
		borderSpawner.spawnRate = hiveMaxSpawnRate;
	}
	
	// Update is called once per frame
	
	public void SetEnemyColor(int id)
	{
		enemyColorId = id;
		GetComponent<Renderer>().material.color = PlayerColor.getColorForId(enemyColorId);
	}

	[PunRPC]
    protected override void Death()
    {
		GameJamGameManager.instance.HiveAboutToBeDestroyed(this);
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
		borderSpawner.spawnRate = (hiveMaxSpawnRate - hiveMinSpawnRate) * healthPercentage + hiveMinSpawnRate;
		transform.localScale = new Vector3(scale, scale, scale);
		GameJamGameManager.instance.totalHiveHealth -= amount;
	}
	void OnPhotonInstantiate(PhotonMessageInfo info) 
	{
		GameJamGameManager gm = GameJamGameManager.instance;
	    // e.g. store this gameobject as this player's charater in PhotonPlayer.TagObject
	    PhotonView pv = GetComponent<PhotonView>();
		SetEnemyColor((int) pv.instantiationData[0]);
		GameJamGameManager.instance.totalHiveStartHealth += startingHealth;
		GameJamGameManager.instance.totalHiveHealth += startingHealth;
	}
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }


}