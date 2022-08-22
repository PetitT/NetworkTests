using Oculus.Platform;
using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusRoomManager : MonoBehaviour
{
    ulong userID;
    ulong roomID;

    private void Start()
    {
        Core.AsyncInitialize().OnComplete(OnInitialize); //This must be called before doing anything
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
            Debug.LogError("You are not entitled to the app..."); //Should kick the player out of the app
        }
        else
        {
            Debug.Log("You are entitled to the app !");
            Users.GetLoggedInUser().OnComplete(OnGotUser); //Returns the current user
        }
    }

    private void OnGotUser(Message<User> message)
    {
        if (message.IsError) { Debug.Log("Couldn't get user"); return; }
        User user = message.GetUser();

        userID = user.ID;
        Debug.Log($"User : {user.DisplayName} - {user.PresenceStatus} - {user.Presence}");

        Rooms.SetRoomInviteReceivedNotificationCallback(OnInviteReceived); //Called on receiver as soon as they receive the invite
    }

    //There should be a validation by the player before doing anything
    private void OnInviteReceived(Message<RoomInviteNotification> message)
    {
        Debug.Log("Received invitation message");
        if (message.IsError) { Debug.Log("Error in invitation"); return; }

        Debug.Log($"Invitation : Room id is {message.Data.RoomID}, invite comes from {message.Data.SenderID}");
        RoomOptions options = new RoomOptions();
        options.SetTurnOffUpdates(false);
        Rooms.Join2(message.Data.RoomID, options).OnComplete(OnJoinedRoom);
    }

    private void OnJoinedRoom(Message<Room> message)
    {
        if (message.IsError) { Debug.Log("Error in joining room"); return; }

        roomID = message.Data.ID;
        Debug.Log($"Joined room {roomID}");
        GetCurrentRoomInfo(message);
        Rooms.SetUpdateNotificationCallback(GetCurrentRoomInfo); //Sends an update whenever 
    }

    public void CreateRoom(string name, uint maxPlayers, RoomJoinPolicy joinPolicy = RoomJoinPolicy.Everyone)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.SetDataStore("Name", name);
        roomOptions.SetTurnOffUpdates(false);
        Rooms.CreateAndJoinPrivate2(joinPolicy, maxPlayers, roomOptions).OnComplete(OnCreatedRoom);
    }

    private void OnCreatedRoom(Message<Room> message)
    {
        Debug.Log($"Created room");
        roomID = message.Data.ID;
        GetCurrentRoomInfo(message);
        Rooms.UpdateOwner(message.Data.ID, userID);
        Rooms.SetUpdateNotificationCallback(GetCurrentRoomInfo);
    }

    public void InviteWithuserFlow()
    {
        Debug.Log("Invitation with user flow");
        Rooms.LaunchInvitableUserFlow(roomID); //Only works on headset, displays a panel with friends to invite
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

    //After a room update notification, the room callback will only contain what has changed,
    //which is why we need to get the room manually to fully update everything
    private void GetCurrentRoomInfo(Message<Room> callback)
    {
        Rooms.GetCurrent().OnComplete((room) => DisplayRoomInfo(room));
    }

    private void DisplayRoomInfo(Message<Room> message)
    {
        if (message.IsError) { return; }
        //Display infos      
    }
}
