using Mirror;
using PlayFab;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayFabIntegration
{

    /// <summary>
    /// This class allows a client to manually request for a server. 
    /// </summary>
    public class ServerRequestManager
    {
        //We need the networkmanager to access the config file, which is shit
        CustomNetworkManager networkManager => NetworkManager.singleton as CustomNetworkManager;

        public void RequestMultiplayerServer(Action<RequestMultiplayerServerResponse> onGotServer)
        {
            if (networkManager == null)
            {
                PlayFabLogging.Log("No custom network manager found");
                return;
            }

            PlayFabLogging.Log("RequestMultiplayerServer");
            var request = new RequestMultiplayerServerRequest
            {
                BuildId = networkManager.Config.buildId,
                SessionId = System.Guid.NewGuid().ToString(),
                PreferredRegions = networkManager.Config.preferredRegions,
                //SessionCookie could be used to send parameters to players
            };

            PlayFabMultiplayerAPI.RequestMultiplayerServer(
                request,
                (response) =>
                {
                    PlayFabLogging.Log("Requested multiplayer server");
                    //networkManager.ConnectToServer(response.IPV4Address, (ushort)response.Ports[0].Num);
                    onGotServer?.Invoke(response);
                },
                (error) =>
                {
                    PlayFabLogging.LogError("Couldn't request multiplayer server", error);
                    onGotServer?.Invoke(null);
                });
        }

    }
}
