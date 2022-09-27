using FishingCactus.Setup;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
using static FishingCactus.Util.Logger;
using static HelperFunctions;

namespace FishingCactus.User
{
    public class PlatformUserSystem : IPlatformUserSystem
    {
        private Settings settings;

        private UniqueUserId UserID;
        private UserOnlineAccount UserAccount;
        private ELoginStatus LoginStatus = ELoginStatus.NotLoggedIn;

        public Task<LoginResult> Login( int controller_id )
        {
            if( !IsControllerValid( controller_id ) )
            {
                return Task.FromResult( new LoginResult( ELoginResult.Failed, new UniqueUserId(), "Invalid controller id" ) );
            }

            if( LoginStatus == ELoginStatus.LoggedIn )
            {
                Log( Util.LogLevel.Warning, "Already logged in" );
                return Task.FromResult( new LoginResult( ELoginResult.Failed, UserID, "Already Logged In" ) );
            }

            Log( Util.LogLevel.Info, "Logging in" );

            var task_completion_source = new TaskCompletionSource<LoginResult>();

            var request = new LoginWithCustomIDRequest
            {
                CustomId = settings.PlayFab.ConnectWithDevice ? SystemInfo.deviceUniqueIdentifier : System.Guid.NewGuid().ToString(),
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true,
                    GetUserData = true
                }
            };

            PlayFabClientAPI.LoginWithCustomID(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Logged in" );
                    UserID = new UniqueUserId( result.PlayFabId );
                    UserAccount = new UserOnlineAccount( UserID );

                    UserAccount.SetUserAttributeByName( result.SessionTicket, StringConstants.SESSION_TICKET );
                    UserAccount.SetUserAttributeByName( result.EntityToken.Entity.Id, StringConstants.ENTITY_ID );
                    UserAccount.SetUserAttributeByName( result.EntityToken.Entity.Type, StringConstants.ENTITY_TYPE );

                    if( result.InfoResultPayload.PlayerProfile != null )
                    {
                        UserAccount.SetUserAttributeByName( result.InfoResultPayload.PlayerProfile.DisplayName, StringConstants.DISPLAY_NAME );
                    }

                    foreach( var item in result.InfoResultPayload.UserData )
                    {
                        UserAccount.SetUserAttributeByName( item.Value.Value, item.Key );
                    }

                    LoginStatus = ELoginStatus.LoggedIn;
                    OnLoginStatusChanged?.Invoke( ELoginStatus.NotLoggedIn, ELoginStatus.LoggedIn, UserID );
                    task_completion_source.TrySetResult( new LoginResult( ELoginResult.SuccessOnlineProfile, UserID, "" ) );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Failed to login :{error.GenerateErrorReport()})" );
                    UserID = new UniqueUserId();
                    UserAccount = new UserOnlineAccount( UserID );
                    task_completion_source.TrySetResult( new LoginResult( ELoginResult.Failed, UserID, error.GenerateErrorReport() ) );
                } );

            return task_completion_source.Task;
        }

        public Task<bool> Logout( int controller_id )
        {
            if( !IsControllerValid( controller_id ) )
            {
                return Task.FromResult( false );
            }


            if( GetLoginStatus( 0 ) == ELoginStatus.NotLoggedIn )
            {
                Log( Util.LogLevel.Warning, "Not logged in" );
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Logged out" );

            PlayFabClientAPI.ForgetAllCredentials();
            UserID = null;
            UserAccount = null;
            LoginStatus = ELoginStatus.NotLoggedIn;
            OnLoginStatusChanged?.Invoke( ELoginStatus.LoggedIn, ELoginStatus.NotLoggedIn, null );
            return Task.FromResult( true );
        }

        public IUserOnlineAccount GetUserAccount( IUniqueUserId user_id )
        {
            if( !IsUserValid( user_id ) )
            {
                return new UserOnlineAccount( new UniqueUserId() );
            }
            return UserAccount;
        }

        public IReadOnlyList<IUserOnlineAccount> GetAllUserAccounts()
        {
            return new ReadOnlyCollection<IUserOnlineAccount>( new List<IUserOnlineAccount> { UserAccount } );
        }

        public IUniqueUserId GetUniqueUserId( int controller_id )
        {
            if( IsControllerValid( controller_id )
                && IsUserValid( UserID ) )
            {
                return UserID;
            }

            return new UniqueUserId();
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

            if( !IsUserValid( user_id ) )
            {
                return ELoginStatus.NotLoggedIn;
            }

            return GetLoginStatus( 0 );
        }

        public ELoginStatus GetLoginStatus( int controller_id )
        {
            if( !IsControllerValid( controller_id ) )
            {
                return ELoginStatus.NotLoggedIn;
            }

            return LoginStatus;
        }

        public string GetPlayerNickname( IUniqueUserId user_id )
        {
            if( !IsUserValid( user_id ) )
            {
                return "";
            }
            return UserAccount.DisplayName;
        }

        public Task<EUserPrivilegesResult> GetUserPrivilege( IUniqueUserId user_id, EUserPrivileges privilege )
        {
            return Task.FromResult( EUserPrivilegesResult.NoFailures );
        }

        public void Initialize( Settings settings )
        {
            this.settings = settings;
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
