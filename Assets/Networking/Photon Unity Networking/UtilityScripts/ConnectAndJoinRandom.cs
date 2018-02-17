using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This script automatically connects to Photon (using the settings file),
/// tries to join a random room and creates one if none was found (which is ok).
/// </summary>
public class ConnectAndJoinRandom : Photon.MonoBehaviour
{
    public static string roomName;
    /// <summary>Connect automatically? If false you can set this to true later on or call ConnectUsingSettings in your own scripts.</summary>

    public byte Version = 1;

    /// <summary>if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()</summary>


    public virtual void Start()
    {
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Random";
        }
        PhotonNetwork.ConnectUsingSettings(Version + "." + SceneManagerHelper.ActiveSceneBuildIndex);
    }

    public virtual void OnConnectedToMaster()
    {
        Debug.LogWarning("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
//        PhotonNetwork.JoinRoom(roomName);
        CreateRoom();
    }

    void JoinRoom()
    {
        PhotonNetwork.JoinRoom(GetRoomName());
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        Debug.LogWarning("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
        CreateRoom();
    }
    int suffix = 1;
    void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = Convert.ToByte(GameJamGameManager.instance.maxNumPlayers) };
        PhotonNetwork.CreateRoom(GetRoomName(), roomOptions, null);
    }

    string GetRoomName()
    {
        return roomName + "_" + suffix;
    }

    // the following methods are implemented to give you some context. re-implement them as needed.

    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError("Cause: " + cause);
    }

    public virtual void OnPhotonCreateRoomFailed()
    {
        Debug.Log("Create room failed. Trying to join room " + GetRoomName());
        if (GameJamGameManager.instance.maxNumPlayers == 1)
        {
            suffix++;
            CreateRoom();
        }
        else
        {
            JoinRoom();
        }
    }

    public virtual void OnPhotonJoinRoomFailed()
    {
        suffix++;
        Debug.Log("join failed. Incrementingn the suffix and creating a room");
        CreateRoom();

    }
    public void OnJoinedRoom()
    {
        Debug.LogWarning("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
    }
}
