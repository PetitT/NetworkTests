using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using TMPro;
using FishingCactus.PlayFabIntegration;
using PlayFab.MultiplayerModels;
using Fusion;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class OculusPhotonPresence : MonoBehaviour
{
    ulong userID;
    string lobbyID;
    public string userName { get; private set; }
    string serverConnectionID;
    public List<TMP_Text> playerTexts;
    public TMP_Text roomText, ownerText, idText, userJoinedText, userInviteText, receivedInviteText, playWithMeText;
    bool loggedIntoPlayFab = false;
    bool loggedIntoOculus = false;
    public float timeToUpdateRoomInfo = 3;
    float remainingTimeToUpdateRoomInfo;
    bool isConnectingToGame = false;
    public NetworkRunner runner;

    void Start()
    {
        Core.AsyncInitialize().OnComplete(OnOculusCoreInitialized);
        PlayFabManager.Instance.LoginManager.LogIn(SystemInfo.deviceUniqueIdentifier, LoginManager_onSuccessfulLogIn);
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(lobbyID) || isConnectingToGame) return;
        remainingTimeToUpdateRoomInfo -= Time.deltaTime;
        if (remainingTimeToUpdateRoomInfo <= 0)
        {
            Debug.Log("Refreshing lobby");
            remainingTimeToUpdateRoomInfo = timeToUpdateRoomInfo;
            PlayFabManager.Instance.LobbyManager.GetLobby(lobbyID, OnGotLobby);
        }
    }

    private void LoginManager_onSuccessfulLogIn(PlayFab.ClientModels.LoginResult obj)
    {
        loggedIntoPlayFab = true;
        CheckPlayFabAndOculusLogin();
    }

    private void CheckPlayFabAndOculusLogin()
    {
        if (loggedIntoOculus && loggedIntoPlayFab)
        {
            if (PlayFabManager.Instance.DisplayName != userName)
            {
                Debug.Log($"Updating display name to {userName}");
                PlayFabManager.Instance.LoginManager.UpdateDisplayName(userName);
            }

            ApplicationLifecycle.SetLaunchIntentChangedNotificationCallback(OnLaunchIntendChanged);
            GroupPresence.SetJoinIntentReceivedNotificationCallback(OnJoinIntent);
        }
    }

    private void OnOculusCoreInitialized(Message message)
    {
        Debug.Log("Initialized");
        Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback); //Check to see wheter the user really owns the app
    }

    private void EntitlementCallback(Message message)
    {
        if (message.IsError)
        {
            Debug.Log("You are not entitled to the app...");
        }
        else
        {
            Debug.Log("You are entitled to the app !");
            Users.GetLoggedInUser().OnComplete(OnGotUser); //Returns the current user (doesn't work in build but works in editor...)
            DisplayLaunchIntent();
        }
    }

    private void OnGotUser(Message<User> message)
    {
        if (message.IsError) { Debug.Log("Couldn't get user"); return; }
        userID = message.GetUser().ID;
        Users.Get(userID).OnComplete(OnGotSelfUser); //Since the message only returns a userID in build, we need to call this method to get the player info
    }

    private void OnGotSelfUser(Message<User> message)
    {
        loggedIntoOculus = true;
        userName = message.GetUser().DisplayName;
        Debug.Log($"User : {userName} - {message.GetUser().PresenceStatus} - {message.GetUser().Presence}");
        CheckPlayFabAndOculusLogin();
    }

    private void OnLaunchIntendChanged(Message<string> message)
    {
        DisplayLaunchIntent();
    }

    private void DisplayLaunchIntent()
    {
        LaunchDetails details = ApplicationLifecycle.GetLaunchDetails();
        receivedInviteText.text = $"{details.LaunchType}";
    }

    private void OnJoinIntent(Message<GroupPresenceJoinIntent> message)
    {
        Debug.Log("Received join intent message");
        GroupPresenceJoinIntent intent = message.Data;
        PlayFabManager.Instance.LobbyManager.JoinLobby(intent.LobbySessionId, (result) =>
        {
            lobbyID = result.LobbyId;
            roomText.text = lobbyID;
        });

        GroupPresenceOptions op = new GroupPresenceOptions();
        op.SetIsJoinable(false);
        op.SetDestinationApiName("lobby_scene");
        op.SetLobbySessionId(intent.LobbySessionId);
        op.SetMatchSessionId("my_match");
        GroupPresence.Set(op);
    }

    public void CreateLobby()
    {
        if (!string.IsNullOrEmpty(lobbyID)) { Debug.Log("You already are in a lobby"); return; }

        PlayFabManager.Instance.LobbyManager.CreateLobby(2, OnCreatedLobby);
    }

    private void OnCreatedLobby(CreateLobbyResult obj)
    {
        lobbyID = obj.LobbyId;
        roomText.text = lobbyID;
        GroupPresenceOptions op = new GroupPresenceOptions();
        op.SetIsJoinable(true);
        op.SetDestinationApiName("lobby_scene");
        op.SetLobbySessionId(obj.ConnectionString);
        op.SetMatchSessionId("my_match");
        GroupPresence.Set(op);
    }

    public void ManualInvite()
    {
        Debug.Log("Manual invitation");
        InviteOptions op = new InviteOptions();
        GroupPresence.LaunchInvitePanel(op);
    }

    public void DisplayPresence()
    {
        RosterOptions op = new RosterOptions();
        GroupPresence.LaunchRosterPanel(op);
    }

    public void ClearPresence()
    {
        GroupPresence.Clear();
    }

    public void LeaveCurrentLobby()
    {
        PlayFabManager.Instance.LobbyManager.LeaveCurrentLobby(
            (result) =>
            {
                if (result)
                {
                    roomText.text = "No Room...";
                    lobbyID = "";
                    DisplayLobbyInfo(null);
                    ClearPresence();
                }
            }
            );
    }

    public void StartGame()
    {
        PlayFabManager.Instance.ServerConnectionManager.RequestMultiplayerServer(OnGotServer);
    }

    private void OnGotServer(RequestMultiplayerServerResponse response)
    {
        string connID = response.SessionId;
        PlayFabManager.Instance.LobbyManager.SetLobbyData(lobbyID, new Dictionary<string, string> { { "Conn", connID } });
    }

    private void OnGotLobby(Lobby obj)
    {
        if (obj == null) return;
        DisplayLobbyInfo(obj);
    }

    private void DisplayLobbyInfo(Lobby lobby)
    {
        if (lobby == null)
        {
            ownerText.text = "Owner";
            playerTexts.ForEach(t => t.text = "...");
            idText.text = "ID...";
            return;
        }

        Debug.Log($"Lobby ID : {lobby.LobbyId}, info : {lobby.Members.Count} users, owner is {lobby.Owner.Id}");

        ownerText.text = $"Owner : {lobby.Owner.Id}";

        for (int i = 0; i < playerTexts.Count; i++)
        {
            if (lobby.Members == null) break;

            if (lobby.Members.Count - 1 >= i)
            {
                playerTexts[i].text = $"Player {i}";
            }
            else
            {
                playerTexts[i].text = "...";
            }
        }

        idText.text = lobby.LobbyId.ToString();

        if (lobby.LobbyData != null)
        {
            if (lobby.LobbyData.ContainsKey("Conn"))
            {
                serverConnectionID = lobby.LobbyData["Conn"];
                Debug.Log($"Attempt to start game. ID : {serverConnectionID}");
                isConnectingToGame = true;
                AwaitClientStart();
            }
        }
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
            SessionName = serverConnectionID,
            SceneManager = runner.GetComponent<INetworkSceneManager>(),
            Scene = SceneManager.GetActiveScene().buildIndex
        });
    }

    public void SimpleJoinServer()
    {
        AwaitClientStart();
    }

    private void OnApplicationQuit()
    {
        PlayFabManager.Instance.LobbyManager.LeaveCurrentLobby();
    }
}