using kcp2k;
using Mirror;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public event Action<string> OnPlayerAdded;
    public event Action<string> OnPlayerRemoved;

    public List<UnityNetworkConnection> Connections { get; set; }

    [SerializeField] Configuration _configuration = default;
    public Configuration Config
    {
        get
        {
            return _configuration;
        }
    }

    public TelepathyTransport Transport
    {
        get
        {
            return transport as TelepathyTransport;
        }
        set
        {
            transport = value;
        }
    }

    public override void Awake()
    {
        base.Awake();

        if (Config.buildType == BuildType.REMOTE_SERVER)
        {
            Connections = new List<UnityNetworkConnection>();
            NetworkServer.RegisterHandler<ReceiveAuthenticateMessage>(OnRecieveAuthenticate);
        }
    }

    private void OnRecieveAuthenticate(NetworkConnection _conn, ReceiveAuthenticateMessage msgType)
    {
        var conn = Connections.Find(c => c.ConnectionId == _conn.connectionId);
        if (conn != null)
        {
            conn.PlayFabId = msgType.PlayFabId;
            conn.IsAuthenticated = true;
            OnPlayerAdded?.Invoke(msgType.PlayFabId);
        }
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (Config.buildType == BuildType.LOCAL_SERVER)
        {
            base.OnServerConnect(conn);
            return;
        }

        var uconn = Connections.Find(c => c.ConnectionId == conn.connectionId);
        if (uconn == null)
        {
            Connections.Add(new UnityNetworkConnection()
            {
                Connection = conn,
                ConnectionId = conn.connectionId,
                LobbyId = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId
            });
        }

    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if(Config.buildType == BuildType.LOCAL_SERVER)
        {
            base.OnServerDisconnect(conn);
            return;
        }

        var uconn = Connections.Find(c => c.ConnectionId == conn.connectionId);
        if (uconn != null)
        {
            if (!string.IsNullOrEmpty(uconn.PlayFabId))
            {
                OnPlayerRemoved?.Invoke(uconn.PlayFabId);
            }
            Connections.Remove(uconn);
        }
    }

    public void ConnectToServer(string networkAddress, ushort port)
    {
        this.networkAddress = networkAddress;
        Transport.port = port;
        StartClient();
    }

}
