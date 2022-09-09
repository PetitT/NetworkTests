using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class ServerManager : MonoBehaviour
{
    private void Start()
    {
        NetworkEvents events = FindObjectOfType<NetworkEvents>();
        events.PlayerJoined.AddListener(OnPlayerJoined);
        events.PlayerLeft.AddListener(OnPlayerLeft);
    }

    private void OnPlayerLeft(NetworkRunner arg0, PlayerRef arg1)
    {
        Debug.Log("Player Left");
    }

    private void OnPlayerJoined(NetworkRunner arg0, PlayerRef arg1)
    {
        Debug.Log("Player Joined");
    }
}
