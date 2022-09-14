using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using TMPro;
using FishingCactus.PlayFabIntegration;
using PlayFab.MultiplayerModels;

public class OculusFriends : MonoBehaviour
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
        Core.AsyncInitialize().OnComplete(OnInitialize); //This must be called before doing anything from Oculus
        PlayFabManager.Instance.LoginManager.LogIn(SystemInfo.deviceUniqueIdentifier, LoginManager_onSuccessfulLogIn);
    }

    private void LoginManager_onSuccessfulLogIn(PlayFab.ClientModels.LoginResult obj)
    {
        loggedIntoPlayFab = true;
        CheckToUpdateName();
    }

    private void CheckToUpdateName()
    {
        if (loggedIntoOculus && loggedIntoPlayFab)
        {
            if (PlayFabManager.Instance.DisplayName != userName)
            {
                Debug.Log($"Updating display name to {userName}");
                PlayFabManager.Instance.LoginManager.UpdateDisplayName(userName);
            }
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
            Users.GetLoggedInUser().OnComplete(OnGotUser); //Returns the current user (doesn't work in build but works in editor...)
            Users.GetLoggedInUserFriends().OnComplete(OnGotFriends); //Returns a list of friends who own the app
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
        Rooms.SetRoomInviteAcceptedNotificationCallback(OnInviteAccepted); //This does not seem to work
        Rooms.SetRoomInviteReceivedNotificationCallback(OnInviteReceived); //Called on receiver as soon as they receive the invite
        CheckToUpdateName();
    }

    private void OnGotFriends(Message<UserList> message)
    {
        Debug.Log($"{message.Data.Count} friend{(message.Data.Count == 1 ? "" : "s")}");
        foreach (var item in message.Data)
        {
            Debug.Log($"{item.DisplayName} - {item.PresenceStatus} - {item.Presence} - {item.InviteToken}");
        }
    }

    //I don't know where or when it should be called...
    private void OnInviteAccepted(Message<string> message)
    {
        Debug.Log("Accept invitation message");
        if (message.IsError) { Debug.Log("Error Message"); return; }

        string roomID = message.GetString();
        Debug.Log($"Room Id is {roomID}");
        receivedInviteText.text = roomID;

        Rooms.Join2(Convert.ToUInt64(roomID), new RoomOptions { });
    }

    //There should be a validation by the player before doing anything
    private void OnInviteReceived(Message<RoomInviteNotification> message)
    {
        Debug.Log("Received invitation message");
        if (message.IsError) { Debug.Log("Error Message"); return; }
        multiplayerPanel.SetActive(true);
        Debug.Log($"Invitation : Room id is {message.Data.RoomID}, Invite comes from {message.Data.SenderID}");
        Users.Get(message.Data.SenderID).OnComplete((user) => playWithMeText.text = $"Play with {user.Data.DisplayName} ?");
        roomID = message.Data.RoomID;
        notificationID = message.Data.ID;
    }

    public void AcceptInvitation()
    {
        multiplayerPanel.SetActive(false);
        RoomOptions options = new RoomOptions();
        options.SetTurnOffUpdates(false);
        Rooms.Join2(roomID, options).OnComplete(OnJoinedRoom);
    }

    public void DeclineInvitation()
    {
        multiplayerPanel.SetActive(false);
        roomID = 0;
        Notifications.MarkAsRead(notificationID);
    }

    private void OnJoinedRoom(Message<Room> message)
    {
        if (message.IsError) { Debug.Log("Error in joining room"); return; }
        Debug.Log($"Joined room {message.Data.ID}");
        GetCurrentRoomInfo(message);
        Rooms.SetUpdateNotificationCallback(GetCurrentRoomInfo);
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.SetDataStore("Name", UnityEngine.Random.Range(100, 999).ToString());
        roomOptions.SetTurnOffUpdates(false);
        Rooms.CreateAndJoinPrivate2(RoomJoinPolicy.Everyone, 10, roomOptions).OnComplete(OnCreatedRoom);
    }

    private void OnCreatedRoom(Message<Room> message)
    {
        Debug.Log($"Created room");
        roomID = message.Data.ID;
        GetCurrentRoomInfo(message);
        Rooms.UpdateOwner(message.Data.ID, userID);
        Rooms.GetInvitableUsers2().OnComplete(OnGotInvitableUsers);
        Rooms.SetUpdateNotificationCallback(GetCurrentRoomInfo);
    }

    private void OnGotInvitableUsers(Message<UserList> message)
    {
        if (message.Data == null) { Debug.Log("No invitable friends"); return; }
        Debug.Log($"{message.Data.Count} invitable friends");
        foreach (var item in message.Data)
        {
            Debug.Log(item.DisplayName);
        }

        inviteToken = message.Data[0].InviteToken;
        userInviteText.text = message.Data[0].DisplayName;
    }

    public void ManualInvite()
    {
        Debug.Log("Manual invitation");
        Rooms.InviteUser(roomID, inviteToken).OnComplete((response) => GetCurrentRoomInfo(response));
        Rooms.GetInvitableUsers2().OnComplete(OnGotInvitableUsers); //An invite token for a user only works once, so we need to reset it after inviting them
    }

    public void InviteWithUserFlow()
    {
        Debug.Log("Invitation with user flow");
        Rooms.LaunchInvitableUserFlow(roomID);
    }

    public void LeaveCurrent()
    {
        Debug.Log("Attempt to leave current room");
        Rooms.GetCurrent().OnComplete((message) =>
        {
            if (message.IsError) return;
            Debug.Log(message.Data.ID);

            Debug.Log("Attempt to leave current");
            Rooms.Leave(message.Data.ID).OnComplete((message) =>
            {
                if (message.IsError) return;
                roomID = 0;
                GetCurrentRoomInfo(message);
                Debug.Log("Left room");
            });
        });
    }

    public void StartGame()
    {
        PlayFabManager.Instance.ServerConnectionManager.RequestMultiplayerServer(OnGotServer);
    }

    private void OnGotServer(RequestMultiplayerServerResponse response)
    {
        ServerConnectionData serverConnectionData = new ServerConnectionData(response.IPV4Address, (ushort)response.Ports[0].Num);
        Rooms.UpdateDataStore(roomID, new Dictionary<string, string> { { "Conn", JsonUtility.ToJson(serverConnectionData) } }).OnComplete((result) => GetCurrentRoomInfo(result));
    }

    private void GetCurrentRoomInfo(Message<Room> callback)
    {
        Rooms.GetCurrent().OnComplete((room) => DisplayRoomInfo(room));
    }

    private void DisplayRoomInfo(Message<Room> message)
    {
        if (message.IsError) { return; }

        Room room = message.Data;

        if (room.DataStore.ContainsKey("Name"))
        {
            roomText.text = $"Room name : {room.DataStore["Name"]}";
        }
        else
        {
            roomText.text = "No Name...";
        }

        Debug.Log($"Room ID : {room.ID}, info : {room.UsersOptional.Count} users, owner is { room.OwnerOptional.DisplayName}");

        ownerText.text = $"Owner : {room.OwnerOptional.DisplayName}";

        if (room.UsersOptional == null)
        {
            player1Text.text = "...";
            player2Text.text = "...";
        }
        else if (room.UsersOptional.Count == 1)
        {
            player1Text.text = room.UsersOptional[0].DisplayName;
            player2Text.text = "Waiting...";
        }
        else if (room.UsersOptional.Count == 2)
        {
            player1Text.text = room.UsersOptional[0].DisplayName;
            player2Text.text = room.UsersOptional[1].DisplayName;
        }

        if (room.InvitedUsersOptional != null)
        {
            invitedText.text = $"Invited : {room.InvitedUsersOptional[0].DisplayName}";
        }
        else
        {
            invitedText.text = "No invited friend ";
        }

        idText.text = room.ID.ToString();

        if (room.DataStore.ContainsKey("Conn"))
        {
            ServerConnectionData data = JsonUtility.FromJson<ServerConnectionData>(room.DataStore["Conn"]);
            FindObjectOfType<CustomNetworkManager>().ConnectToServer(data.ipv4Address, data.port);
        }
    }

    private void OnApplicationQuit()
    {
        if (roomID != 0)
        {
            Rooms.Leave(roomID);
        }
    }
}