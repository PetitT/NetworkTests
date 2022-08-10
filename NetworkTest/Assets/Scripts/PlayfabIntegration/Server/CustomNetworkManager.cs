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
        Connections = new List<UnityNetworkConnection>();
        NetworkServer.RegisterHandler<ReceiveAuthenticateMessage>(OnRecieveAuthenticate);
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
        base.OnServerConnect(conn);
        var uconn = Connections.Find(c => c.ConnectionId == conn.connectionId);
        if (uconn == null)
        {
            string LobbyID = "";
            if (_configuration.buildType == BuildType.REMOTE_SERVER)
            {
                LobbyID = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId;
            }

            Connections.Add(new UnityNetworkConnection()
            {
                Connection = conn,
                ConnectionId = conn.connectionId,
                LobbyId = LobbyID
            });
        }

        Debug.Log($"Player connected : {conn.connectionId}");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        var uconn = Connections.Find(c => c.ConnectionId == conn.connectionId);
        if (uconn != null)
        {
            OnPlayerRemoved?.Invoke(uconn.PlayFabId);
            Connections.Remove(uconn);
        }

        Debug.Log($"Player disconnected : {conn.connectionId}");

        if (Connections.Count == 0)
        {
            Debug.Log("No connected player anymore");
            StartCoroutine(ShutDown());
        }
    }

    public void ConnectToServer(string networkAddress, ushort port)
    {
        this.networkAddress = networkAddress;
        Transport.port = port;
        StartClient();
    }

    public IEnumerator ShutDown()
    {
        for (int i = 3; i > 0; i++)
        {
            Debug.Log($"Server shutting down in {i}");
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Shutting down...");
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }
}
