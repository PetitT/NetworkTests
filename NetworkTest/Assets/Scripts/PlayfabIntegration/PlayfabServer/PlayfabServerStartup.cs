using PlayFab;
using PlayFab.MultiplayerAgent.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FishingCactus.PlayFabIntegration
{
    public class PlayfabServerStartup : MonoBehaviour
    {
        //FIELDS
        [SerializeField] float serverShutdownTimer = 500f;
        [SerializeField] bool debugging = true;
        List<ConnectedPlayer> connectedPlayers = new List<ConnectedPlayer>(); //A list of players that is sent to keep playfab updated, don't use it for gameplay
        string sessionID;

        public event Action onServerStarted;
        //PROPERTIES
        public Configuration Config => PlayFabManager.Instance.Configuration;

        //METHODS 
        public void StartRemoteServer()
        {
            PlayFabMultiplayerAgentAPI.Start();
            PlayFabLogging.LogWarning($"[SERVER STARTUP] START PLAYFAB AGENT");
            PlayFabMultiplayerAgentAPI.IsDebugging = debugging;
            PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
            PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
            PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
            PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;

            StartCoroutine( ReadyForPlayers() );
            StartCoroutine( ShutdownServerInXTime( serverShutdownTimer ) );
        }

        private void OnServerActive()
        {
            PlayFabLogging.LogWarning( "[SERVER STARTUP] PLAYFAB SERVER STARTED FROM AGENT ACTIVATION" );
            onServerStarted?.Invoke();
        }

        private void AddPlayer( string playerID )
        {
            ConnectedPlayer player = new ConnectedPlayer( playerID );
            connectedPlayers.Add( player );
            PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers( connectedPlayers );
        }

        private void OnPlayerLeft( string playerID )
        {
            ConnectedPlayer player = connectedPlayers.FirstOrDefault( player => player.PlayerId == playerID );
            connectedPlayers.Remove( player );
            PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers( connectedPlayers );
            if ( connectedPlayers.Count <= 0 )
            {
                PlayFabLogging.LogWarning( "[SERVER STARTUP] NO MORE PLAYERS, SHUTTING DOWN SERVER" );
                StartShutdownProcess();
            }
        }

        private void OnAgentError( string error )
        {
            PlayFabLogging.LogWarning( error );
        }

        private void OnShutdown()
        {
            PlayFabLogging.LogWarning( "[SERVER STARTUP] REQUEST SHUTDOWN" );
            StartShutdownProcess();
        }

        private void OnMaintenance( DateTime? NextScheduledMaintenanceUtc )
        {
            PlayFabLogging.LogWarning( "[SERVER STARTUP] REQUEST MAINTENANCE" );
            StartShutdownProcess();
        }

        private void StartShutdownProcess()
        {
            StartCoroutine( Shutdown() );
        }

        IEnumerator ReadyForPlayers()
        {
            yield return new WaitForSeconds( 1 );
            PlayFabMultiplayerAgentAPI.ReadyForPlayers();
            PlayFabLogging.LogWarning( "[SERVER STARTUP] SET READY FOR PLAYERS" );
        }

        IEnumerator ShutdownServerInXTime( float time = 300f )
        {
            yield return new WaitForSeconds( time );
            StartShutdownProcess();
        }

        IEnumerator Shutdown()
        {
            PlayFabLogging.LogWarning( "[SERVER STARTUP] SHUTTING DOWN" );
            yield return new WaitForSeconds( 5f );
            Application.Quit();
        }

        //UNITY
        private void Start()
        {
            if ( Config.buildType == BuildType.REMOTE_SERVER )
            {
                PlayFabLogging.LogWarning( "[SERVER STARTUP] BEGIN OF REMOTE SERVER" );
                StartRemoteServer();
            }
        }

        private void OnDisable()
        {
            if ( Config.buildType == BuildType.REMOTE_SERVER )
            {
                //CleanUpRemoteServer();
            }
        }
    }
}