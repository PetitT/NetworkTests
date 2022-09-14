using PlayFab;
using PlayFab.ClientModels;
using System;

namespace FishingCactus.PlayFabIntegration
{
    public class LoginManager
    {
        public bool IsLoggedIn => !string.IsNullOrEmpty(SessionTicket);
        public string DisplayName { get; private set; }
        public string PlayFabID { get; private set; }
        public string SessionTicket { get; private set; }
        public EntityKey EntityKey { get; private set; }

        public void LogIn(
            string id,
            Action<LoginResult> onLoggedIn = null
            )
        {
            PlayFabLogging.Log("Attempt to log in");

            if (IsLoggedIn)
            {
                PlayFabLogging.Log("Already logged in");
                onLoggedIn?.Invoke(null);
                return;
            }

            var request = new LoginWithCustomIDRequest
            {
                CustomId = id,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };

            PlayFabClientAPI.LoginWithCustomID(
                request,
                (result) =>
                {
                    OnLoggedIn(result);
                    onLoggedIn?.Invoke(result);
                },
                (error) =>
                {
                    PlayFabLogging.LogError("Failed to login", error);
                    onLoggedIn?.Invoke(null);
                });
        }

        private void OnLoggedIn(LoginResult result)
        {
            PlayFabLogging.Log("Successful login!");
            EntityKey = result.EntityToken.Entity;
            PlayFabID = result.PlayFabId;
            SessionTicket = result.SessionTicket;

            if (result.InfoResultPayload.PlayerProfile != null) //This will be null if the account was just created
            {
                string newDisplayName = result.InfoResultPayload.PlayerProfile.DisplayName;
                if (!string.IsNullOrEmpty(newDisplayName))
                {
                    DisplayName = newDisplayName;
                }
            }
        }

        public void UpdateDisplayName( string newName )
        {
            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = newName
            };

            PlayFabClientAPI.UpdateUserTitleDisplayName(
                request,
                ( result ) =>
                {
                    PlayFabLogging.Log($"Successfully updated display name to { result.DisplayName }");
                    DisplayName = result.DisplayName;
                },
                ( error ) => PlayFabLogging.LogError("Failed to update display name", error)
                );
        }
    }
}
