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
				PhotonNetwork.InstantiateSceneObject(playerPrefab.name, Vector3.zero, Quaternion.identity, 0, parameters);				
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
			Debug.Log("Waiting for more players to join");
		}
	}

	void StartGame()
	{
		if (gameStarted)
		{
			return;
		}

		gameStarted = true;
		int color = UnityEngine.Random.Range(1, 3);
		foreach(Transform t in hiveStartingPoints)
		{
			object[] parameters = new object[] {color};
			PhotonNetwork.InstantiateSceneObject(hivePrefab.name, t.position, Quaternion.identity, 0, parameters);				
			color = color == 1 ? 2 : 1;
		}
		PauseManager.instance.Unpause();
	}

	public void KillPlayer(Player player)
	{
		foreach(Player p in players)
		{
			if (player == p)
			{
				PhotonNetwork.Destroy(player.gameObject);
			}
		}

		if (player.id == LocalPlayerId)
		{

		}
		players.Remove(player);
	}

	void CreatePlayer()
	{
		Instantiate(playerPrefab);
	}
}
