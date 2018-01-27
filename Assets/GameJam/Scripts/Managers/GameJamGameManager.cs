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
	public GameObject playerPrefab;
	GameObject playersParent;
	public List<Player> players = new List<Player>();
	public int maxNumPlayers = 2;
	int numPlayers = 0;
	PlayerRoomIndexing indexer;
	
	void Awake()
	{
		PauseManager.instance.Pause();
		indexer = GameObject.FindObjectOfType<PlayerRoomIndexing>();
		indexer.OnRoomIndexingChanged.AddListener(UpdatePlayers);
		playersParent = new GameObject("Players");
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

		if (numPlayers > ogNumPlayers) //players added here
		{
			if (playersParent.transform.childCount < numPlayers)
			{
				GameObject playerGO = Instantiate(playerPrefab);
				playerGO.name = "Player " + realPlayers.Count;
				playerGO.transform.SetParent(playersParent.transform);
				Player player = playerGO.GetComponent<Player>();
				player.damageImage = GameObject.Find("DamageImage").GetComponent<Image>();
				player.healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
				player.id = realPlayers[realPlayers.Count-1];
				players.Add(player);
				if (player.id == LocalPlayerId)
				{
					CameraFollow.instance.target = player.transform;
				}
			}
		}
		
		if (numPlayers == maxNumPlayers)
		{
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
