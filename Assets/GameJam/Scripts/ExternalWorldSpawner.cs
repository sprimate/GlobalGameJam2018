using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalWorldSpawner : BorderSpawner {
	public float maxSpawnRate;
	public float minSpawnRate;

	void Start()
	{
		spawnRate = maxSpawnRate;
	}

	protected override void Update()
	{
		GameJamGameManager m = GameJamGameManager.instance;
		if (m.totalHiveStartHealth != 0)
		{
			spawnRate = m.totalHiveHealth <= 0 ? 0 : (maxSpawnRate - minSpawnRate) * ((float)m.totalHiveHealth / (float)m.totalHiveStartHealth) + minSpawnRate;
		}
		base.Update();
	}
}