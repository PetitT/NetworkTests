using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using FishingCactus.PlayFabIntegration;
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

    void Start()
    {
        if (Room.Config.Type == Configuration.BuildType.CLIENT)
        {
            _room.OnPlayerAdded += OnConnected;
            _room.OnPlayerRemoved += OnDisconnected;

            NetworkClient.RegisterHandler<ShutdownMessage>(OnServerShutDown);
            NetworkClient.RegisterHandler<MaintenanceMessage>(OnMaintenanceMessage);
        }
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

    private void OnMaintenanceMessage(MaintenanceMessage message)
    {
        Debug.Log($"Maintenance is scheduled for: {message.ScheduledMaintenanceUTC.ToString("MM-DD-YYYY hh:mm:ss")}");
    }

    private void OnServerShutDown(ShutdownMessage msg)
    {
        Debug.Log("Server has issued a shutdown.");
        NetworkClient.Disconnect();
    }
#endif
}