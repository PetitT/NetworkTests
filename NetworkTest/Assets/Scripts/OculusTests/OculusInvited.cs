using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OculusInvited : MonoBehaviour
{
    public TMP_Text receivedStringText;

    void Start()
    {
        Oculus.Platform.Rooms.SetRoomInviteAcceptedNotificationCallback(OnInviteAccepted);
    }

    private void OnInviteAccepted(Message<string> message)
    {
        if (message.IsError) { return; }

        string roomID = message.GetString();
        receivedStringText.text = roomID;

        RoomOptions options = new RoomOptions { };
        Rooms.Join2(Convert.ToUInt64(roomID), options);
    }
}
