using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using TMPro;
using PlayFabIntegration;
using PlayFab.MultiplayerModels;

public class OculusPresence : MonoBehaviour
{
    ulong userID;
    string lobbyID;
    string userName;
    public List<TMP_Text> playerTexts;
    public TMP_Text roomText, ownerText, idText, userJoinedText, userInviteText, receivedInviteText, playWithMeText;
    bool loggedIntoPlayFab = false;
    bool loggedIntoOculus = false;
    public float timeToUpdateRoomInfo = 3;
    float remainingTimeToUpdateRoomInfo;
    bool isConnectedToGame = false;

    void Start()
    {
        Core.AsyncInitialize().OnComplete(OnOculusCoreInitialized);
        PlayFabManager.Instance.LoginManager.LogIn();
        PlayFabManager.Instance.LoginManager.onSuccessfulLogIn += LoginManager_onSuccessfulLogIn;
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(lobbyID) || isConnectedToGame) return;
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
        receivedInviteText.text = $"{details.LaunchType}, {details.RoomID}";
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

        PlayFabManager.Instance.LobbyManager.CreateLobby(OnCreatedLobby);
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
        GroupPresence.LaunchInvitePanel(op); //TRY TO FILTER BY FRIENDS OWNING THE APP
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
        ServerConnectionData serverConnectionData = new ServerConnectionData(response.IPV4Address, (ushort)response.Ports[0].Num);
        PlayFabManager.Instance.LobbyManager.SetLobbyData(lobbyID, new Dictionary<string, string> { { "Conn", JsonUtility.ToJson(serverConnectionData) } });
    }

    private void OnGotLobby(GetLobbyResult obj)
    {
        if (obj == null) return;
        DisplayLobbyInfo(obj.Lobby);
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
                playerTexts[i].text = lobby.Members[i].MemberEntity.Id;
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
                ServerConnectionData data = JsonUtility.FromJson<ServerConnectionData>(lobby.LobbyData["Conn"]);
                Debug.Log($"Attempt to start game. IP : {data.ipv4Address}, Port : {data.port} ");
                isConnectedToGame = true;
                FindObjectOfType<CustomNetworkManager>().ConnectToServer(data.ipv4Address, data.port);
            }
        }
    }

    private void OnApplicationQuit()
    {
        PlayFabManager.Instance.LobbyManager.LeaveCurrentLobby();
    }
}