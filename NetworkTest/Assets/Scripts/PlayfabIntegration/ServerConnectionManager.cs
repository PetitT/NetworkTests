using Mirror;
using PlayFab;
using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class allows a client to manually request for a server. It serves as testing purposes, as a client should not be able to do that in production
/// </summary>
public class ServerConnectionManager
{
    CustomNetworkManager networkManager => NetworkManager.singleton as CustomNetworkManager;

    public void RequestMultiplayerServer()
    {
        if(networkManager == null)
        {
            Debug.Log("No custom network manager found");
            return;
        }

        Debug.Log("RequestMultiplayerServer");
        RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
        requestData.BuildId = networkManager.Config.buildId;
        requestData.SessionId = System.Guid.NewGuid().ToString();
        requestData.PreferredRegions = networkManager.Config.preferredRegions;

        PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
    }

    private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
    {
        Debug.Log("Requested multiplayer server");
        networkManager.ConnectToServer(response.IPV4Address, (ushort)response.Ports[0].Num);
    }

    private void OnRequestMultiplayerServerError(PlayFabError error)
    {
        Debug.Log($"Couldn't request multiplayer server : {error.GenerateErrorReport()}");
    }
}
