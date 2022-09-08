using Fusion;
using Fusion.Sockets;
using PlayFab.MultiplayerModels;
using PlayFabIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonTest : MonoBehaviour
{
    string joinString;
    string sessionName;
    ushort port = 27015;

    float timeToPoll;
    bool connecting = false;

    private void Update()
    {
        if (PlayFabManager.Instance.LobbyManager.IsInALobby && !connecting)
        {
            timeToPoll -= Time.deltaTime;
            if (timeToPoll < 0)
            {
                PollCurrentLobby();
                timeToPoll = 7;
            }
        }
    }

    private void OnLobbyCreated(CreateLobbyResult obj)
    {
        joinString = obj.ConnectionString;
    }

    private void StartAsServer()
    {
        NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        runner.StartGame(new StartGameArgs
        {
            Address = NetAddress.Any(port),
            GameMode = GameMode.Server,
            SessionName = sessionName,
            Initialized = OnInitialize,
            SceneManager = runner.GetComponent<INetworkSceneManager>(),
            Scene = SceneManager.GetActiveScene().buildIndex
        });
    }

    private void OnInitialize(NetworkRunner obj)
    {
        Debug.Log("Initialized");
    }

    private async void AwaitClientStart()
    {
        var result = await StartAsClient();
        if (result.Ok)
        {
            Debug.Log("Started as client");
        }
        else
        {
            Debug.Log($"Couldn't start client : {result.ShutdownReason}");
        }
    }

    private Task<StartGameResult> StartAsClient()
    {
        NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        return runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = runner.GetComponent<INetworkSceneManager>(),
            Scene = SceneManager.GetActiveScene().buildIndex
        });
    }

    private void RequestServer()
    {
        PlayFabManager.Instance.ServerConnectionManager.RequestMultiplayerServer(OnRequestedServer);
    }

    private void OnRequestedServer(RequestMultiplayerServerResponse obj)
    {
        sessionName = obj.SessionId;
        PlayFabManager.Instance.LobbyManager.SetCurrentLobbyData(new Dictionary<string, string> { { "conn", sessionName } });
    }

    private void PollCurrentLobby()
    {
        PlayFabManager.Instance.LobbyManager.GetCurrentLobby(OnPolledLobby);
    }

    private void OnPolledLobby(GetLobbyResult obj)
    {
        if (obj.Lobby.LobbyData == null) { Debug.Log("No Lobby data"); return; }
        if (obj.Lobby.LobbyData.ContainsKey("conn"))
        {
            sessionName = obj.Lobby.LobbyData["conn"];
            Debug.Log($"Found connection string {sessionName}. Trying to connect");
            connecting = true;
            AwaitClientStart();
        }
    }

    private void OnGUI()
    {
        if (!PlayFabManager.Instance.IsLoggedIn)
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Device Login"))
            {
                PlayFabManager.Instance.LoginManager.LogIn(LoginManager.LoginMethod.DeviceID);
            }
            if (GUI.Button(new Rect(0, 50, 100, 50), "Random Login"))
            {
                PlayFabManager.Instance.LoginManager.LogIn(LoginManager.LoginMethod.Random);
            }
        }
        else
        {
            GUI.Label(new Rect(0, 0, 100, 20), "Join String");
            joinString = GUI.TextField(new Rect(100, 0, 500, 20), joinString);

            GUI.Label(new Rect(0, 20, 100, 20), "Sess Name");
            sessionName = GUI.TextField(new Rect(100, 20, 500, 20), sessionName);

            if (GUI.Button(new Rect(0, 40, 100, 50), "Create Lobby"))
            {
                PlayFabManager.Instance.LobbyManager.CreateLobby(OnLobbyCreated);
            }

            if (GUI.Button(new Rect(100, 40, 100, 50), "Join Lobby"))
            {
                PlayFabManager.Instance.LobbyManager.JoinLobby(joinString);
            }
            if (GUI.Button(new Rect(0, 90, 100, 50), "Request Server"))
            {
                RequestServer();
            }

            if (GUI.Button(new Rect(0, 300, 100, 50), "Start Server"))
            {
                StartAsServer();
            }
            if (GUI.Button(new Rect(100, 300, 100, 50), "Start Client"))
            {
                AwaitClientStart();
            }
        }
    }
}
