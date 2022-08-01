using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace PlayFabIntegration
{
    public class LoginManager
    {
        public event Action onSuccessfulLogIn;
        public event Action onFailedToLogIn;
        public event Action<string> onUpdatedDisplayName;

        public bool IsLoggedIn { get; private set; }
        public string DisplayName { get; private set; }

        #region LOGIN
        public void LogInWithDeviceID()
        {
            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };

            PlayFabClientAPI.LoginWithCustomID(request, OnLoggedIn, OnError);
        }

        private void OnLoggedIn(LoginResult result)
        {
            Debug.Log("Successful login!");
            IsLoggedIn = true;
            onSuccessfulLogIn?.Invoke();
            DisplayName = result.InfoResultPayload.PlayerProfile.DisplayName;
        }

        private void OnError(PlayFabError error)
        {
            Debug.Log($"Failed to login : {error.GenerateErrorReport()}");
            onFailedToLogIn?.Invoke();
        }

        #endregion

        #region DISPLAY NAME
        public void UpdateDisplayName(string newName)
        {
            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = newName
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUpdatedDisplayName, OnFailedToUpdateDisplayName);
        }

        private void OnUpdatedDisplayName(UpdateUserTitleDisplayNameResult result)
        {
            Debug.Log($"Successfully updated display name to { result.DisplayName}");
            DisplayName = result.DisplayName;
            onUpdatedDisplayName?.Invoke(DisplayName);
        }

        private void OnFailedToUpdateDisplayName(PlayFabError error)
        {
            Debug.Log($"Failed to update display name : {error.GenerateErrorReport()}");
        }
        #endregion
    }
}
