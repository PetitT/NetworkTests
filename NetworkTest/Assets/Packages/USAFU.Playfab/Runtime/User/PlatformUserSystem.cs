using FishingCactus.Setup;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;

namespace FishingCactus.User
{
    public class PlatformUserSystem : IPlatformUserSystem
    {
        private UniqueUserId UserID;
        private UserOnlineAccount UserAccount;

        private bool IsLoggedIn => UserAccount.GetUserAttribute( StringConstants.ENTITY_ID, out string result);

        public Task<LoginResult> Login( int controller_id )
        {
            Util.Logger.Log( Util.LogLevel.Info, "Logging in" );
            var taskCompletionSource = new TaskCompletionSource<LoginResult>();

            if( IsLoggedIn )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Already logged in" );
                taskCompletionSource.TrySetResult( new LoginResult( ELoginResult.Failed, UserID, "Already Logged In" ) );
            }

            var request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };

            PlayFabClientAPI.LoginWithCustomID(
                request,
                ( result ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Info, "Logged in" );
                    UserID = new UniqueUserId( result.PlayFabId );
                    UserAccount = new UserOnlineAccount( UserID );

                    UserAccount.SetUserAttributeByName( result.SessionTicket, StringConstants.SESSION_TICKET );
                    UserAccount.SetUserAttributeByName( result.InfoResultPayload.PlayerProfile.DisplayName, StringConstants.DISPLAY_NAME );
                    UserAccount.SetUserAttributeByName( result.EntityToken.Entity.Id, StringConstants.ENTITY_ID );
                    UserAccount.SetUserAttributeByName( result.EntityToken.Entity.Type, StringConstants.ENTITY_TYPE );

                    taskCompletionSource.TrySetResult( new LoginResult( ELoginResult.SuccessOnlineProfile, UserID, "" ) );
                },
                ( error ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Error, $"Failed to login :{error.GenerateErrorReport()})" );
                    UserID = new UniqueUserId();
                    UserAccount = new UserOnlineAccount( UserID );
                    taskCompletionSource.TrySetResult( new LoginResult( ELoginResult.Failed, UserID, error.GenerateErrorReport() ) );
                } );

            return taskCompletionSource.Task;
        }

        public Task<bool> Logout( int controller_id )
        {
            return Task.FromResult( true );
        }

        public IUserOnlineAccount GetUserAccount( IUniqueUserId user_id )
        {
            return UserAccount;
        }

        public IReadOnlyList<IUserOnlineAccount> GetAllUserAccounts()
        {
            return new ReadOnlyCollection<IUserOnlineAccount>( new List<IUserOnlineAccount> { UserAccount } );
        }

        public IUniqueUserId GetUniqueUserId( int controller_id )
        {
            return UserID;
        }

        public IUniqueUserId CreateUniqueUserId( byte[] bytes, int size )
        {
            return UserID;
        }

        public IUniqueUserId CreateUniqueUserId( string textual_representation )
        {
            return UserID;
        }

        public ELoginStatus GetLoginStatus( IUniqueUserId user_id )
        {
            return GetLoginStatus( 0 );
        }

        public ELoginStatus GetLoginStatus( int controller_id )
        {
            return ELoginStatus.UsingLocalProfile;
        }

        public string GetPlayerNickname( IUniqueUserId user_id )
        {
            return UserAccount.DisplayName;
        }

        public Task<EUserPrivilegesResult> GetUserPrivilege( IUniqueUserId user_id, EUserPrivileges privilege )
        {
            return Task.FromResult( EUserPrivilegesResult.NoFailures );
        }

        public void Initialize( Settings settings )
        {
        }

        public Task<GetUserAvatarResult> GetUserAvatar( IUniqueUserId user_id, AvatarSize avatar_size )
        {
            return Task.FromResult( new GetUserAvatarResult { Success = false, Avatar = null } );
        }

#pragma warning disable CS0067 // The event 'event' is never used
        public event OnControllerPairingChangedDelegate OnControllerPairingChanged;
        public event OnLoginStatusChangedDelegate OnLoginStatusChanged;
#pragma warning restore CS0067 // The event 'event' is never used
    }
}
