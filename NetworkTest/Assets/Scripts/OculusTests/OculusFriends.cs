using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using TMPro;

public class OculusFriends : MonoBehaviour
{
    ulong userID;
    ulong roomID;
    string inviteToken;

    public TMP_Text roomText, player1Text, player2Text, invitedText, ownerText, idText, userJoinedText, userInviteText;

    void Start()
    {
        Core.AsyncInitialize().OnComplete(OnInitialize);
        Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
    }

    private void EntitlementCallback(Message message)
    {
        if (message.IsError) { Debug.Log("You are not entitled to the app..."); }
        else { Debug.Log("You are entitled to the app !"); }
    }

    private void OnInitialize(Message message)
    {
        Debug.Log("Attempt to initialize");
        //Users.GetLoggedInUserFriends().OnComplete(OnGotFriends); //Returns a list of friends who own the app
        Users.GetLoggedInUser().OnComplete(OnGotUser);
        StartCoroutine(UpdateRoomStatus());
    }

    private void OnGotFriends(Message<UserList> message)
    {
        Debug.Log($"{message.Data.Count} friend{(message.Data.Count == 1 ? "" : "s")}");
        foreach (var item in message.Data)
        {
            Debug.Log($"{item.DisplayName} - {item.PresenceStatus} - {item.Presence} - {item.InviteToken}");
        }
    }

    private void OnGotUser(Message<User> message)
    {
        if (message.IsError) { Debug.Log("Couldn't get user"); return; }
        User user = message.GetUser();

        userID = user.ID;
        Debug.Log($"User : {user.DisplayName} - {user.PresenceStatus} - {user.Presence}");
        Rooms.GetCurrentForUser(user.ID).OnComplete(OnGotCurrentRoom);
    }

    private void OnGotCurrentRoom(Message<Room> message)
    {
        if (message.IsError)
        {
            Debug.Log("No room");
            return;
        }

        Debug.Log(message.Data.UsersOptional[0].DisplayName);
    }

    public void CreateRoom()
    {
        RoomOptions options = new RoomOptions();
        Rooms.CreateAndJoinPrivate2(RoomJoinPolicy.Everyone, 10, options).OnComplete(OnCreatedRoom);
    }

    private void OnCreatedRoom(Message<Room> message)
    {
        Debug.Log($"Created room");
        roomID = message.Data.ID;
        Rooms.UpdateOwner(roomID, userID);
        Rooms.UpdateDataStore(roomID, new Dictionary<string, string>() { { "Name", UnityEngine.Random.Range(100, 999).ToString() } });
        Rooms.GetInvitableUsers2().OnComplete(OnGotInvitableUsers);
    }

    private void OnGotInvitableUsers(Message<UserList> message)
    {
        if(message.Data == null) { Debug.Log("No invitable friends"); return; }
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
        Rooms.InviteUser(roomID, inviteToken);
    }

    private void InviteWithuserFlow()
    {
        Debug.Log("ATTEMPT TO INVITE");
        Rooms.LaunchInvitableUserFlow(roomID);
    }

    public void LeaveCurrent()
    {
        Rooms.GetCurrent().OnComplete((message) =>
        {
            if (message.IsError) return;
            Debug.Log(message.Data.ID);

            Debug.Log("Attempt to leave current");
            Rooms.Leave(message.Data.ID).OnComplete((message) =>
            {
                if (message.IsError) return;
                roomID = 0;
                Debug.Log("Left room");
            });
        });

    }

    private IEnumerator UpdateRoomStatus()
    {
        for (int i = 0; i < 1000; i++)
        {
            Rooms.GetCurrent().OnComplete(DisplayRoomInfo);
            yield return new WaitForSeconds(2);
        }
    }

    private void DisplayRoomInfo(Message<Room> message)
    {
        if (message.IsError) { return; }

        Room room = message.Data;
        string roomName = "";

        if (room.DataStore.ContainsKey("Name"))
        {
            roomName = room.DataStore["Name"];
            roomText.text = $"Room name : {room.DataStore["Name"]}";
        }
        else
        {
            roomText.text = "";
        }

        Debug.Log($"Room -{roomName}- , ID : {room.ID}, info : {room.UsersOptional.Count} users, owner is { room.OwnerOptional.DisplayName}");

        ownerText.text = $"Owner : {room.OwnerOptional.DisplayName}";

        if (room.UsersOptional == null)
        {
            player1Text.text = "";
            player2Text.text = "";
        }
        if (room.UsersOptional.Count >= 1)
        {
            player1Text.text = room.UsersOptional[0].DisplayName;
        }
        if (room.UsersOptional.Count == 2)
        {
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
    }
}
