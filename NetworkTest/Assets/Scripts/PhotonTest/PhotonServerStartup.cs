using Fusion;
using Fusion.Sockets;
using PlayFab;
using PlayFab.MultiplayerAgent.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonServerStartup : MonoBehaviour
{
    [SerializeField] float _serverShutdownTimer = 500f;
    [SerializeField] bool _debugging = true;

    private List<ConnectedPlayer> _connectedPlayers; //A list of players that is sent to keep playfab updated, don't use it for gameplay
    private NetworkRunner runner;

    public Configuration Config;
    string sessionID;

    private void Start()
    {
        if(Config.buildType == BuildType.REMOTE_SERVER)
        {
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
        PlayFabMultiplayerAgentAPI.IsDebugging = _debugging;
        //PlayFabMultiplayerAgentAPI.OnMaintenanceCallback += OnShutdown;
        PlayFabMultiplayerAgentAPI.OnShutDownCallback += OnShutdown;
        PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
        PlayFabMultiplayerAgentAPI.OnAgentErrorCallback += OnAgentError;
        sessionID = PlayFabMultiplayerAgentAPI.SessionConfig.SessionId;

        StartCoroutine(ShutdownServerInXTime(_serverShutdownTimer));
    }

    private void OnServerActive()
    {
        StartServer();
        Debug.Log("Server Started From Agent Activation");
    }

    public void StartServer()
    {
        runner.StartGame(new StartGameArgs
        {
            Address = NetAddress.Any(7777),
            GameMode = GameMode.Server,
            SessionName = sessionID,
            SceneManager = runner.GetComponent<INetworkSceneManager>(),
            Initialized = OnInitialize,
        });
    }

    private void OnInitialize(NetworkRunner obj)
    {
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
    }

    private void OnPlayerRemoved(string playfabId)
    {
        ConnectedPlayer player = _connectedPlayers.Find(x => x.PlayerId.Equals(playfabId, StringComparison.OrdinalIgnoreCase));
        _connectedPlayers.Remove(player);
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
    }

    private void OnPlayerAdded(string playfabId)
    {
        _connectedPlayers.Add(new ConnectedPlayer(playfabId));
        PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_connectedPlayers);
    }

    private void OnAgentError(string error)
    {
        Debug.Log(error);
    }

    private void OnShutdown()
    {
        StartShutdownProcess();
    }

    private void StartShutdownProcess()
    {
        Debug.Log("Server is shutting down");
        StartCoroutine(Shutdown());
    }

    IEnumerator Shutdown()
    {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    IEnumerator ShutdownServerInXTime(float time = 300f)
    {
        yield return new WaitForSeconds(time);
        StartShutdownProcess();
    }
}
