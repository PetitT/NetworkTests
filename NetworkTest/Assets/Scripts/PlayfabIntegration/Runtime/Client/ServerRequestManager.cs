using PlayFab;
using PlayFab.MultiplayerModels;
using System;
using System.Linq;

namespace FishingCactus.PlayFabIntegration
{
    public class ServerRequestManager
    {
        public void RequestMultiplayerServer( Action<RequestMultiplayerServerResponse> onGotServer )
        {
            PlayFabLogging.Log( "Attempting to get a multiplayer server" );

            var request = new RequestMultiplayerServerRequest
            {
                BuildId = PlayFabManager.Instance.Configuration.buildId,
                SessionId = Guid.NewGuid().ToString(),
                PreferredRegions = PlayFabManager.Instance.Configuration.preferredRegions.ToList()
            };

            PlayFabMultiplayerAPI.RequestMultiplayerServer(
                request,
                ( response ) =>
                {
                    PlayFabLogging.Log( "Found a multiplayer server" );                
                    onGotServer?.Invoke( response );
                },
                ( error ) =>
                {
                    PlayFabLogging.LogError( "Couldn't request multiplayer server", error );
                    onGotServer?.Invoke( null );
                });
        }

    }
}
