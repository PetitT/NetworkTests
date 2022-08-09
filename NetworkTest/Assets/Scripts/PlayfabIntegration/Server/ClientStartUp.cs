using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFabIntegration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientStartUp : MonoBehaviour
{
#if !UNITY_SERVER
    protected CustomNetworkManager _room;
    protected CustomNetworkManager Room
    {
        get
        {
            if (_room != null) { return _room; }
            return _room = NetworkManager.singleton as CustomNetworkManager;
        }
    }

    public List<string> preferredRegions = new List<string>();

    void Start()
    {
        if (Room.Config.buildType == BuildType.LOCAL_CLIENT || Room.Config.buildType == BuildType.REMOTE_CLIENT)
        {
            //PlayFabManager.Instance.LoginManager.onSuccessfulLogIn += OnLoginSuccess;

            _room.OnPlayerAdded += OnConnected;
            _room.OnPlayerRemoved += OnDisconnected;

            NetworkClient.RegisterHandler<ShutdownMessage>(OnServerShutDown);
            NetworkClient.RegisterHandler<MaintenanceMessage>(OnMaintenanceMessage);
        }
    }

    private void OnMaintenanceMessage(MaintenanceMessage message)
    {
        Debug.Log($"Maintenance is scheduled for: {message.ScheduledMaintenanceUTC.ToString("MM-DD-YYYY hh:mm:ss")}");
    }

    private void OnServerShutDown(ShutdownMessage msg)
    {
        Debug.Log("Server has issued a shutdown.");
        NetworkClient.Disconnect();
    }

    private void OnConnected(string msg)
    {
        Debug.Log("You are connected to the server");

        NetworkClient.connection.Send(new ReceiveAuthenticateMessage()
        {
            PlayFabId = PlayFabManager.Instance.PlayfabID
        });
    }

    private void OnDisconnected(string msg)
    {
        Debug.Log("You were disconnected from the server");
    }

    private void OnLoginSuccess(LoginResult success)
    {
        Debug.Log($"You logged in successfully. ID: {success.PlayFabId}");

        if (string.IsNullOrWhiteSpace(Room.Config.ipAddress))
        {
            //We need to grab an IP and Port from a server based on the buildId. Copy this and add it to your Configuration.
            RequestMultiplayerServer();
        }
        else
        {
            ConnectRemoteClient();
        }
    }

    private void ConnectRemoteClient(RequestMultiplayerServerResponse response = null)
    {
        if (response == null)
        {
            Room.networkAddress = Room.Config.ipAddress;
            Room.Transport.port = Room.Config.port;
        }
        else
        {
            Debug.Log("**** ADD THIS TO YOUR CONFIGURATION **** -- IP: " + response.IPV4Address + " Port: " + (ushort)response.Ports[0].Num);
            Room.networkAddress = response.IPV4Address;
            Room.Transport.port = (ushort)response.Ports[0].Num;
        }

        Room.StartClient();
    }

    private void RequestMultiplayerServer()
    {
        Debug.Log("[ClientStartUp].RequestMultiplayerServer");
        RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
        requestData.BuildId = Room.Config.buildId;
        requestData.SessionId = System.Guid.NewGuid().ToString();
        requestData.PreferredRegions = preferredRegions;

        PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
    }

    private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
    {
        Debug.Log("Requested multiplayer server");
        ConnectRemoteClient(response);
    }

    private void OnRequestMultiplayerServerError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

#endif
}