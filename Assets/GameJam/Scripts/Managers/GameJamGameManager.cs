using System;
using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using ExitGames.UtilityScripts;
using UnityEngine;
using UnityEngine.UI;

public class GameJamGameManager : MonoSingleton<GameJamGameManager> {

    public static int LocalPlayerId { get {
            return PhotonNetwork.player.ID;
        } }

	public bool allowGhostMode = true;
    public List<Soul> souls;
    int destroyedEnemies;
    int destroyedHives;
    public float hiveRegenerationTime;
    bool gameStarted = false;
    public Transform[] hiveStartingPoints;
    public string playerLayerName;
    public GameObject playerPrefab;
    public GameObject hivePrefab;
    GameObject playersParent;
    public List<Player> players { get; set; }
    public static int? maxNumPlayersOverride;
	public int maxNumPlayers = 2;
	int numPlayers = 0;
	bool waitForAllPlayers = true;
	PlayerRoomIndexing indexer;
	IList<Hive> hives = new List<Hive>();
	public int totalHiveHealth;
	public int totalHiveStartHealth;
    bool swapped = false;
	public Transform underworldFloor;
	void Awake()
	{
        souls = new List<Soul>();
        players = new List<Player>();
        if (maxNumPlayersOverride.HasValue)
        {
            maxNumPlayers = maxNumPlayersOverride.Value;
        }
		if (maxNumPlayers == 1)
		{
			waitMessage = "Initializing";
		}
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

    public void ClearSouls()
    {
        foreach (var soul in souls)
        {
            Destroy(soul);
        }
        souls.Clear();
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
		else if (!gameStarted)
		{
			displayWaitingnForPlayersMessage = true;
		}
	}
	string waitMessage = "Waiting for more players to join";
	bool displayWaitingnForPlayersMessage = true;
	void OnGUI()
	{
		if (displayWaitingnForPlayersMessage)
		{
			var centeredStyle = GUI.skin.GetStyle("Label");
   			centeredStyle.alignment = TextAnchor.UpperCenter;
			centeredStyle.fontSize = 50;
			GUI.Label(new Rect(50, Screen.height/2-25, Screen.width-50, Screen.height), waitMessage, centeredStyle);
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

    public int GetClosestTargetId(Vector3 position)
    {
        Player closestPlayer = null;
        float closestDistance = float.MaxValue;
        foreach (Player p in players)
        {
            try
            {
                if (p.isDead)
                {
                    continue;
                }
                var dist = Vector3.Distance(p.transform.position, position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPlayer = p;
                }
            }
            catch (Exception)
            {
                continue;
            }
        }

        if (closestPlayer == null)
        {
            return 0;
        }
		
        if (swapped)
        {
            return closestPlayer.id == 1 ? 2 : 1;
        }

        return closestPlayer.id;
    }

    public void Swap()
    {
        swapped = !swapped;
    }

    public Player GetTarget(int targetId)
    {
        try
        {
            if (swapped)
            {
                targetId = targetId == 1 ? 2 : 1;
            }
            if (players[targetId - 1] == null || players[targetId - 1].isDead)
            {
                return players[targetId == 1 ? 1 : 0];
            }
            return players[targetId - 1];
        }
        catch (Exception)
        {
            return null;
        }
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

    public bool retargetEnemies = false;
    public void TriggerEnemyTargetRecalculation()
    {

        StartCoroutine(TriggerRetarget());
    }

    IEnumerator TriggerRetarget()
    {
        retargetEnemies = true;
        yield return null;
        yield return new WaitForEndOfFrame();
        retargetEnemies = false;
    }
}
