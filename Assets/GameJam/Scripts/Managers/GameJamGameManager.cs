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
	public string playerLayerName;
	public GameObject playerPrefab;
	GameObject playersParent;
	public List<Player> players = new List<Player>();
	public int maxNumPlayers = 2;
	int numPlayers = 0;
	public bool waitForAllPlayers = false;
	PlayerRoomIndexing indexer;
	
	void Awake()
	{
		PhotonNetwork.autoCleanUpPlayerObjects = false;
		if (waitForAllPlayers)
		{
			PauseManager.instance.Pause();
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
		
		if (numPlayers == maxNumPlayers)
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
		PauseManager.instance.Unpause();
	}

	void CreatePlayer()
	{
		Instantiate(playerPrefab);
	}
}
