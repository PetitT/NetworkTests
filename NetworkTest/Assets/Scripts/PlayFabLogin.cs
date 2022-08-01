using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabLogin : MonoBehaviour
{
    public bool loginOnStart;
    public string ID;
    public string displayName;

    void Start()
    {
        if (loginOnStart)
        {
            Login();
        }
    }

    [ContextMenu("Login")]
    public void Login()
    {
        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest
        {
            //CustomId = SystemInfo.deviceUniqueIdentifier,
            CustomId = ID,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }            
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
    }

    private void OnSuccess(LoginResult result)
    {
        Debug.Log("Successful login!");
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log($"Failed to login : {error.GenerateErrorReport()}");
    }

    [ContextMenu("Update display name")]
    public void UpdateDisplayName()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUpdateDisplayName, OnFailedToUpdateDisplayName);
    }

    private void OnUpdateDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log($"Successfully updated display name to { result.DisplayName}");
    }

    private void OnFailedToUpdateDisplayName(PlayFabError error)
    {
        Debug.Log($"Failed to update display name : {error.GenerateErrorReport()}");
    }
}
