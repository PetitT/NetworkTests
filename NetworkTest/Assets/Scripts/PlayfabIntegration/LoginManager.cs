using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace PlayFabIntegration
{
    /// <summary>
    /// The login manager allows one to connect to a playfab account and to set their display name
    /// </summary>
    public class LoginManager
    {
        public event Action<LoginResult> onSuccessfulLogIn;
        public event Action onFailedToLogIn;
        public event Action<string> onUpdatedDisplayName;

        public bool IsLoggedIn { get; private set; }
        public string DisplayName { get; private set; }
        public string LoggedInPlayFabID { get; private set; }
        public string EntityID { get; private set; }
        public string SessionTicket { get; private set; }
        public EntityKey EntityKey { get; private set; }
        public enum LoginMethod { DeviceID, Random }

        #region CLIENT LOGIN
        /// <summary>
        /// Logs the player to an account. 
        /// </summary>
        /// <param name="method">Logging with Device ID means there is one account per device. Logging with random ID serves as testing tool</param>
        public void LogInWithID(LoginMethod method = LoginMethod.DeviceID)
        {
            if (IsLoggedIn)
            {
                PlayFabLogging.Log("Already logged in");
                return;
            }

            string ID = "";

            switch (method)
            {
                case LoginMethod.DeviceID:
                    ID = SystemInfo.deviceUniqueIdentifier;
                    break;
                case LoginMethod.Random:
                    ID = UnityEngine.Random.Range(10000, 99999).ToString();
                    break;

                default:
                    break;
            }

            var request = new LoginWithCustomIDRequest
            {
                CustomId = ID,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true                    
                }
            };

            PlayFabClientAPI.LoginWithCustomID(
                request,
                OnLoggedIn,
                OnError
                );
        }

        private void OnLoggedIn(LoginResult result)
        {
            PlayFabLogging.Log("Successful login!");
            EntityKey = result.EntityToken.Entity;            
            IsLoggedIn = true;
            LoggedInPlayFabID = result.PlayFabId;
            SessionTicket = result.SessionTicket;
            EntityID = result.EntityToken.Entity.Id;

            if (result.InfoResultPayload.PlayerProfile != null) //This will be null if the account was just created
            {
                string newDisplayName = result.InfoResultPayload.PlayerProfile.DisplayName;
                if (!string.IsNullOrEmpty(newDisplayName))
                {
                    DisplayName = newDisplayName;
                }
            }

            onSuccessfulLogIn?.Invoke(result);
        }

        private void OnError(PlayFabError error)
        {
            PlayFabLogging.LogError("Failed to login" , error);
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

            PlayFabClientAPI.UpdateUserTitleDisplayName(
                request,
                OnUpdatedDisplayName,
                (error) => PlayFabLogging.LogError("Failed to update display name", error)
                );
        }

        private void OnUpdatedDisplayName(UpdateUserTitleDisplayNameResult result)
        {
            PlayFabLogging.Log($"Successfully updated display name to { result.DisplayName}");
            DisplayName = result.DisplayName;
            onUpdatedDisplayName?.Invoke(DisplayName);
        }
        #endregion
    }
}
