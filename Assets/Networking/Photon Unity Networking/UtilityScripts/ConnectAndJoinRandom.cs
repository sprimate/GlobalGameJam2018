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
    public bool AutoConnect = true;

    public byte Version = 1;

    /// <summary>if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()</summary>
    private bool ConnectInUpdate = true;


    public virtual void Start()
    {
        PhotonNetwork.autoJoinLobby = false;    // we join randomly. always. no need to join a lobby to get the list of rooms.
    }

    public virtual void Update()
    {
        if (ConnectInUpdate && AutoConnect && !PhotonNetwork.connected)
        {
            Debug.Log("Update() was called by Unity. Scene is loaded. Let's connect to the Photon Master Server. Calling: PhotonNetwork.ConnectUsingSettings();");

            ConnectInUpdate = false;
            PhotonNetwork.ConnectUsingSettings(Version + "." + SceneManagerHelper.ActiveSceneBuildIndex);
        }
    }


    // below, we implement some callbacks of PUN
    // you can find PUN's callbacks in the class PunBehaviour or in enum PhotonNetworkingMessage


    public virtual void OnConnectedToMaster()
    {
        Debug.LogWarning("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
//        PhotonNetwork.JoinRoom(roomName);
        JoinRoom();
    }

    public virtual void OnJoinedLobby()
    {
        Debug.LogWarning("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
        JoinRoom();
    }

    void JoinRoom()
    {
        if (roomName == null)
        {
            Debug.Log("Joining Random Room");
            PhotonNetwork.JoinRandomRoom();
            return;
        }
        foreach (var r in PhotonNetwork.GetRoomList())
        {
            if (r.PlayerCount >= r.MaxPlayers)
            {
                continue;
            }
            int indexOf = r.Name.IndexOf(roomName);
            if (indexOf != 0)
            {
                continue;
            }
            int thisSuffix = -1;
            if (r.Name.IndexOf("_") == indexOf + roomName.Length && int.TryParse(r.Name.Substring(roomName.Length + 1), out thisSuffix))
            {
                Debug.Log("Joining room " + r.Name);
                PhotonNetwork.JoinRoom(r.Name);
            }
        }
        CreateRoom();

    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        Debug.LogWarning("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");

        CreateRoom();
    }

    void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = Convert.ToByte(GameJamGameManager.instance.maxNumPlayers) };
        if (roomName == null)
        {
            Debug.Log("Creating Random Room: " + roomOptions);
            PhotonNetwork.CreateRoom(null, roomOptions, null);
            return;
        }
        int suffix = 1;
        List<String> names = new List<string>();
        foreach (var r in PhotonNetwork.GetRoomList())
        {
            names.Add(r.Name);
        }
        do
        {
            if (!names.Contains(roomName + suffix))
            {
                Debug.Log("Creating Room " + GetRoomName(suffix));
                PhotonNetwork.CreateRoom(GetRoomName(suffix), roomOptions, null);
                return;
            }
            suffix++;
        } while (suffix <= names.Count);
        Debug.LogError("Unable to create room from given name " + roomName + " for some unknown reason");
//        PhotonNetwork.CreateRoom(null, roomOptions, null);

    }

    string GetRoomName(int num)
    {
        return roomName + "_" + num;
    }

    // the following methods are implemented to give you some context. re-implement them as needed.

    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError("Cause: " + cause);
    }

    public void OnJoinedRoom()
    {
        Debug.LogWarning("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
    }
}
