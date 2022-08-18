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
    /// This class allows a client to manually request for a server. It serves as testing purposes, as a client should not be able to do that in production
    /// </summary>
    public class ServerRequestManager
    {
        CustomNetworkManager networkManager => NetworkManager.singleton as CustomNetworkManager;

        public void RequestMultiplayerServer()
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
                PreferredRegions = networkManager.Config.preferredRegions
            };

            PlayFabMultiplayerAPI.RequestMultiplayerServer(
                request,
                OnRequestMultiplayerServer,
                (error) => PlayFabLogging.LogError("Couldn't request multiplayer server", error)
                );
        }

        private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
        {
            PlayFabLogging.Log("Requested multiplayer server");
            networkManager.ConnectToServer(response.IPV4Address, (ushort)response.Ports[0].Num);
        }
    }
}
