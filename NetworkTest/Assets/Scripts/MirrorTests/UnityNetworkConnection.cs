using Mirror;
using PlayFab.MultiplayerAgent.Model;
using System;

[Serializable]
public class UnityNetworkConnection
{
    public bool IsAuthenticated;
    public string PlayFabId;
    public string LobbyId;
    public int ConnectionId;
    public NetworkConnection Connection;
}
