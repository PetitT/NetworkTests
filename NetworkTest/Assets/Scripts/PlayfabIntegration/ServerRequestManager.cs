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

        public void RequestMultiplayerServer(Action<RequestMultiplayerServerResponse> onGotServer)
        {

            PlayFabLogging.Log("RequestMultiplayerServer");
            var request = new RequestMultiplayerServerRequest
            {
                BuildId = PlayFabManager.Instance.Config.buildId,
                SessionId = System.Guid.NewGuid().ToString(),
                PreferredRegions = PlayFabManager.Instance.Config.preferredRegions
            };

            PlayFabMultiplayerAPI.RequestMultiplayerServer(
                request,
                (response) =>
                {
                    PlayFabLogging.Log("Requested multiplayer server");                
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
