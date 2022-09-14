using Fusion;
using Fusion.Sockets;
using PlayFab;
using PlayFab.MultiplayerAgent.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishingCactus.PlayFabIntegration
{
    public class PhotonServerStartup : MonoBehaviour
    {
        [SerializeField] float _serverShutdownTimer = 500f;
        [SerializeField] bool _debugging = true;

        private List<ConnectedPlayer> _connectedPlayers; //A list of players that is sent to keep playfab updated, don't use it for gameplay
        public NetworkRunner runner;

        public Configuration Config;
        string sessionID;

        private void Start()
        {
            if (Config.buildType == BuildType.REMOTE_SERVER)
            {
                Debug.LogWarning("[SERVER STARTUP] BEGIN OF REMOTE SERVER");
                StartRemoteServer();
            }
        }

        private void OnDisable()
        {
            if (Config.buildType == BuildType.REMOTE_SERVER)
            {
                //CleanUpRemoteServer();
            }
        }

        public void StartRemoteServer()
        {
            _connectedPlayers = new List<ConnectedPlayer>();
            PlayFabMultiplayerAgentAPI.Start();
            Debug.LogWarning($"[SERVER STARTUP] START PLAYFAB AGENT");
            PlayFabMultiplayerAgentAPI.IsDebugging = _debugging;
            PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnMaintenance;
            PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
            PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
            PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;
            NetworkEvents networkEvents = FindObjectOfType<NetworkEvents>();
            networkEvents.PlayerJoined.AddListener(OnPlayerJoined);
            networkEvents.PlayerLeft.AddListener(OnPlayerLeft);


            StartCoroutine(ReadyForPlayers());
            StartCoroutine(ShutdownServerInXTime(_serverShutdownTimer));
        }

        private void OnServerActive()
        {
            Debug.LogWarning("[SERVER STARTUP] PLAYFAB SERVER STARTED FROM AGENT ACTIVATION");
            AwaitForServerStart();
        }

        private async void AwaitForServerStart()
        {
            var result = await StartServer();
            if (result.Ok)
            {
                Debug.LogWarning("[SERVER STARTUP] PHOTON SERVER SUCCESFULLY STARTED");
            }
            else
            {
                Debug.LogWarning("[SERVER STARTUP] PHOTON SERVER SUCCESFULLY STARTED");
            }

            Debug.LogWarning("[SERVER STARTUP] SET REDY FOR PLAYERS");
            PlayFabMultiplayerAgentAPI.ReadyForPlayers();
        }

        private Task<StartGameResult> StartServer()
        {
            sessionID = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId;
            Debug.LogWarning($"[SERVER STARTUP] STARTING PHOTON SERVER, ID IS : {sessionID}");
            if (runner == null) { runner = FindObjectOfType<NetworkRunner>(); }

            return runner.StartGame(new StartGameArgs
            {
                Address = NetAddress.Any(27015),
                GameMode = GameMode.Server,
                SessionName = sessionID,
                SceneManager = runner.GetComponent<INetworkSceneManager>(),
                Scene = SceneManager.GetActiveScene().buildIndex,
            });
        }

        private void OnPlayerJoined(NetworkRunner arg0, PlayerRef arg1)
        {
            ConnectedPlayer player = new ConnectedPlayer(arg1.PlayerId.ToString());
            _connectedPlayers.Add(player);
            PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
        }

        private void OnPlayerLeft(NetworkRunner arg0, PlayerRef arg1)
        {
            ConnectedPlayer player = _connectedPlayers.FirstOrDefault(player => player.PlayerId == arg1.PlayerId.ToString());
            _connectedPlayers.Remove(player);
            PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
            if (_connectedPlayers.Count == 0)
            {
                Debug.LogWarning("[SERVER STARTUP] NO MORE PLAYERS, SHUTTING DOWN SERVER");
                StartShutdownProcess();
            }
        }

        private void OnAgentError(string error)
        {
            Debug.Log(error);
        }

        private void OnShutdown()
        {
            Debug.LogWarning("[SERVER STARTUP] REQUEST SHUTDOWN");
            StartShutdownProcess();
        }

        private void OnMaintenance(DateTime? NextScheduledMaintenanceUtc)
        {
            Debug.LogWarning("[SERVER STARTUP] REQUEST MAINTENANCE");
            StartShutdownProcess();
        }

        private void StartShutdownProcess()
        {
            StartCoroutine(Shutdown());
        }

        IEnumerator Shutdown()
        {
            Debug.LogWarning("[SERVER STARTUP] SHUTTING DOWN");
            yield return new WaitForSeconds(5f);
            Application.Quit();
        }

        IEnumerator ReadyForPlayers()
        {
            yield return new WaitForSeconds(1);
            PlayFabMultiplayerAgentAPI.ReadyForPlayers();
            Debug.LogWarning("[SERVER STARTUP] SET READY FOR PLAYERS");
        }

        IEnumerator ShutdownServerInXTime(float time = 300f)
        {
            yield return new WaitForSeconds(time);
            StartShutdownProcess();
        }
    }
}
