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
    ulong roomID;
    ulong notificationID;
    string userName;
    string inviteToken;

    public TMP_Text roomText, player1Text, player2Text, invitedText, ownerText, idText, userJoinedText, userInviteText, receivedInviteText, playWithMeText;
    public GameObject multiplayerPanel;
    bool loggedIntoPlayFab = false;
    bool loggedIntoOculus = false;

    void Start()
    {
        Core.AsyncInitialize().OnComplete(OnInitialize); //This must be called before doing anything
        PlayFabManager.Instance.LoginManager.LogInWithID();
        PlayFabManager.Instance.LoginManager.onSuccessfulLogIn += LoginManager_onSuccessfulLogIn;
    }

    private void LoginManager_onSuccessfulLogIn(PlayFab.ClientModels.LoginResult obj)
    {
        loggedIntoPlayFab = true;
        CheckPlayfabAndOculusLogin();
    }

    private void CheckPlayfabAndOculusLogin()
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

    private void OnInitialize(Message message)
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
            Users.GetLoggedInUser().OnComplete(OnGotUser); //Returns the current user (doesn't work in build but works in editor...
            DisplayLaunchIntent();
        }
    }

    private void OnGotUser(Message<User> message)
    {
        if (message.IsError) { Debug.Log("Couldn't get user"); return; }
        userID = message.GetUser().ID;
        Users.Get(userID).OnComplete(OnGotSelfUser); //Since the message only returns a userID in build, we need to call this method to actually get the player info
    }

    private void OnGotSelfUser(Message<User> message)
    {
        loggedIntoOculus = true;
        userName = message.GetUser().DisplayName;
        Debug.Log($"User : {userName} - {message.GetUser().PresenceStatus} - {message.GetUser().Presence}");
        CheckPlayfabAndOculusLogin();
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
        ownerText.text = intent.LobbySessionId;

        PlayFabManager.Instance.LobbyManager.JoinLobby(intent.LobbySessionId, OnJoinLobby);
    }

    private void OnJoinLobby(JoinLobbyResult obj)
    {
        if (obj == null)
        {
            Debug.Log("Couldn't join lobby...");
            return;
        }
        Debug.Log("Joined lobby");
        PlayFabManager.Instance.LobbyManager.GetLobby(obj.LobbyId, OnGotLobby);
    }

    private void OnGotLobby(GetLobbyResult obj)
    {
        DisplayLobbyInfo(obj.Lobby);
    }

    public void AcceptInvitation()
    {
        multiplayerPanel.SetActive(false);
    }

    public void DeclineInvitation()
    {
        multiplayerPanel.SetActive(false);
    }

    public void CreateLobby()
    {
        PlayFabManager.Instance.LobbyManager.CreateLobby(OnCreatedLobby);
    }

    private void OnCreatedLobby(CreateLobbyResult obj)
    {
        roomText.text = obj.LobbyId;
        GroupPresenceOptions op = new GroupPresenceOptions();
        op.SetIsJoinable(true);
        op.SetDestinationApiName("lobby_scene");
        op.SetLobbySessionId(obj.ConnectionString);
        op.SetMatchSessionId("my_match");
        GroupPresence.Set(op);
        PlayFabManager.Instance.LobbyManager.GetLobby(obj.LobbyId, OnGotLobby);
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

    public void LeaveCurrent()
    {
        PlayFabManager.Instance.LobbyManager.LeaveCurrentLobby(
            (result) => { if (result) { roomText.text = "No Room..."; } }
            );
    }

    public void StartGame()
    {
        PlayFabManager.Instance.ServerConnectionManager.RequestMultiplayerServer(OnGotServer);
    }

    private void OnGotServer(RequestMultiplayerServerResponse response)
    {
        ServerConnectionData serverConnectionData = new ServerConnectionData(response.IPV4Address, (ushort)response.Ports[0].Num);
    }


    private void DisplayLobbyInfo(Lobby lobby)
    {
        Debug.Log($"Lobby ID : {lobby.LobbyId}, info : {lobby.Members.Count} users, owner is {lobby.Owner.Id}");

        ownerText.text = $"Owner : {lobby.Owner.Id}";

        if (lobby.Members.Count == 0)
        {
            player1Text.text = "...";
            player2Text.text = "...";
        }

        else if (lobby.Members.Count == 1)
        {
            player1Text.text = lobby.Members[0].MemberEntity.Id;
            player2Text.text = "Waiting...";
        }
        else if (lobby.Members.Count == 2)
        {
            player1Text.text = lobby.Members[0].MemberEntity.Id;
            player2Text.text = lobby.Members[1].MemberEntity.Id;
        }

        idText.text = lobby.LobbyId.ToString();

        //if (room.DataStore.ContainsKey("Conn"))
        //{
        //    ServerConnectionData data = JsonUtility.FromJson<ServerConnectionData>(room.DataStore["Conn"]);
        //    FindObjectOfType<CustomNetworkManager>().ConnectToServer(data.ipv4Address, data.port);
        //}
    }

    private void OnApplicationQuit()
    {
        if (roomID != 0)
        {
            Rooms.Leave(roomID);
        }
    }
}