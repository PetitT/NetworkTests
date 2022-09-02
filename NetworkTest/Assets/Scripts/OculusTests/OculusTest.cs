using Oculus.Platform;
using Oculus.Platform.Models;
using OculusIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFabIntegration;
using PlayFab.ClientModels;

public class OculusTest : MonoBehaviour
{
    string lobbyID;

    private void Start()
    {
        OculusManager.Instance.StartupManager.AsyncInitializeCore(OnCoreInitialized);
        PlayFabManager.Instance.LoginManager.LogInWithID(onLoggedIn: OnPlayfabLogin);
    }

    private void OnCoreInitialized(Message<PlatformInitialize> message)
    {
        OculusManager.Instance.StartupManager.CheckEntitlement(OnEntitlementChecked);
        CheckLogins();
    }

    private void OnPlayfabLogin(LoginResult result)
    {
        CheckLogins();
    }

    private void OnEntitlementChecked(bool result)
    {
        if (!result)
        {
            Debug.LogError("NOT ENTITLED I SHOULD KICK YOU");
        }
        OculusManager.Instance.UsersManager.InitializeLoggedInUserInfo(OnGotUser);
        GroupPresence.SetJoinIntentReceivedNotificationCallback(OnJoinIntent);
    }

    private void OnGotUser(User result)
    {
        CheckLogins();
    }

    private void CheckLogins()
    {
        if (OculusManager.Instance.Initialized && PlayFabManager.Instance.IsLoggedIn)
        {
            string oculusDisplayName = OculusManager.Instance.UserName;
            if (PlayFabManager.Instance.DisplayName != oculusDisplayName)
            {
                Debug.Log($"Updating display name to {oculusDisplayName}");
                PlayFabManager.Instance.LoginManager.UpdateDisplayName(oculusDisplayName);
            }
        }
    }

    private void OnJoinIntent(Message<GroupPresenceJoinIntent> message)
    {
        Debug.Log("Received join intent message");
        GroupPresenceJoinIntent intent = message.Data;
        JoinGroupPresence(intent.LobbySessionId);
        JoinLobby(intent.LobbySessionId);
    }

    private static void JoinGroupPresence(string lobbySessionID)
    {
        OculusManager.Instance.GroupPresenceManager.SetGroupPresence(
            false,
            "lobby_scene",
            lobbySessionID,
            "my_match"
            );
    }

    private void JoinLobby(string lobbyID)
    {
        PlayFabManager.Instance.LobbyManager.JoinLobby(
            lobbyID,
            result => { lobbyID = result.LobbyId; }
            );
    }

    public void InviteFriends()
    {
        OculusManager.Instance.GroupPresenceManager.LaunchInvitePanel();
    }
}
