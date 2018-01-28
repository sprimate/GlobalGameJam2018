using System;
using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using ExitGames.UtilityScripts;
using UnityEngine;
using UnityEngine.UI;

public class GameJamGameManager : MonoSingleton<GameJamGameManager> {

	public static int LocalPlayerId {get {
		return PhotonNetwork.player.ID;
	}}
	int destroyedEnemies;
	int destroyedHives;
	public float hiveRegenerationTime;
	bool gameStarted = false;
	public Transform[] hiveStartingPoints;
	public string playerLayerName;
	public GameObject playerPrefab;
	public GameObject hivePrefab;
	GameObject playersParent;
	public List<Player> players = new List<Player>();
	public int maxNumPlayers = 2;
	int numPlayers = 0;
	public bool waitForAllPlayers = false;
	PlayerRoomIndexing indexer;
	IList<Hive> hives = new List<Hive>();
	public int totalHiveHealth;
	public int totalHiveStartHealth;
	void Awake()
	{
		PhotonNetwork.autoCleanUpPlayerObjects = false;
		if (waitForAllPlayers)
		{
			PauseManager.instance.Pause();
		}
		else
		{
			StartGame();
		}
		try{
			indexer = GameObject.FindObjectOfType<PlayerRoomIndexing>();
			indexer.OnRoomIndexingChanged.AddListener(UpdatePlayers);
			playersParent = new GameObject("Players");
		}
		catch(Exception)
		{}
	}
	public void UpdatePlayers()
	{
		var ogNumPlayers = numPlayers;
		List<int> realPlayers = new List<int>();
		numPlayers = 0;
		foreach(var id in indexer.PlayerIds)
		{
			if (id != 0)
			{
				realPlayers.Add(id);
				numPlayers++;
			}
		}

		//numPlayers++;
		if (numPlayers > maxNumPlayers)
		{
			return;
		}

		if (PhotonNetwork.player.IsMasterClient && numPlayers > ogNumPlayers) //players added here
		{
			if (playersParent.transform.childCount < numPlayers)
			{
				int id = realPlayers[realPlayers.Count-1];
				object[] parameters = new object[] {id};
				PhotonNetwork.InstantiateSceneObject(playerPrefab.name, Vector3.zero, Quaternion.identity, 0, parameters).GetComponent<Hive>();
				//Instantiate(playerPrefab);
			}
		}
		
		if (waitForAllPlayers && numPlayers == maxNumPlayers)
		{
			Debug.Log("Going to start game with player ids: "+realPlayers);
			StartGame();
		}
		else
		{
			displayWaitingnForPlayersMessage = true;
		}
	}

	bool displayWaitingnForPlayersMessage;
	void OnGUI()
	{
		if (displayWaitingnForPlayersMessage)
		{
			GUI.Label(new Rect(10, 10, 100, 20), "Waiting for more players to join");
		}
	}

	void StartGame()
	{
		displayWaitingnForPlayersMessage = false;
		if (gameStarted)
		{
			return;
		}

		gameStarted = true;
		int color = UnityEngine.Random.Range(1, 3);
		foreach(Transform t in hiveStartingPoints)
		{
			CreateHive(color, t.position);			
			color = color == 1 ? 2 : 1;

		}
		PauseManager.instance.Unpause();
	}

	void CreateHive(int color, Vector3 position)
	{
		if (PhotonNetwork.isMasterClient)
		{
			object[] parameters = new object[] {color};
			PhotonNetwork.InstantiateSceneObject(hivePrefab.name, position, Quaternion.identity, 0, parameters);	
		}			
	}

	public void HiveAboutToBeDestroyed(Hive h)
	{
		GameObject.Find("HivesDestroyed").GetComponent<Text>().text = "Hives: " + ++destroyedHives;
		int newHiveColor = h.enemyColorId;
		if (h.numChanges % 2 != 0 )
		{
			newHiveColor = newHiveColor == 1 ? 2 : 1;
		}
		StartCoroutine(RegenerateHive(newHiveColor, h.transform.position));
	}

	public void RecordEnemyDestroyed()
	{
		GameObject.Find("EnemiesDestroyed").GetComponent<Text>().text = "Enemies: " + ++destroyedEnemies;
	}

	IEnumerator RegenerateHive(int color, Vector3 position)
	{
		yield return new WaitForSeconds(hiveRegenerationTime);
		CreateHive(color, position);
	}
	void CreatePlayer()
	{
		Instantiate(playerPrefab);
	}
}
