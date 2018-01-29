using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateRoomName : MonoBehaviour {

    public void OnValueChanged(string val)
    {
        ConnectAndJoinRandom.roomName = val;
    }
}
