using Mirror;
using PlayFab;
using PlayFab.MultiplayerModels;
using PlayFabIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabClient : NetworkManager
{
    public override void Start()
    {
        base.Start();
        FindObjectOfType<PlayFabManager>().LoginManager.onSuccessfulLogIn += RequestMultiplayerServer;
    }

    private void RequestMultiplayerServer()
    {
        RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();

        requestData.BuildId = "f387e43b-edfd-4fd9-b4fe-5f675c6190e2";
        requestData.PreferredRegions = new List<string>() { "NorthEurope" };
        //requestData.SessionId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
        requestData.SessionId = System.Guid.NewGuid().ToString();
        PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
    }

    private void OnRequestMultiplayerServerError(PlayFabError obj)
    {
        throw new NotImplementedException();
    }

    private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
    {
        this.networkAddress = response.IPV4Address;
        this.GetComponent<TelepathyTransport>().port = (ushort)response.Ports[0].Num;
        this.StartClient();
    }
}
