using FishingCactus.Setup;
using Oculus.Platform;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class PlatformUserSystem : IPlatformUserSystem
    {
        private UserOnlineAccount userOnlineAccount;
        private UniqueUserId uniquerUserID;

        private ELoginStatus loginStatus = ELoginStatus.NotLoggedIn;

        public Task<LoginResult> Login( int controller_id )
        {
            if( controller_id < 0
                && controller_id >= USAFUCore.Get().Platform.MaxLocalPlayers
                )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid Controller ID" );
                return Task.FromResult( new LoginResult( ELoginResult.Failed, new UniqueUserId(), "Invalid Controller ID" ) );
            }

            if( uniquerUserID != null )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Already logged in" );
                return Task.FromResult( new LoginResult( ELoginResult.Failed, new UniqueUserId(), "Already logged in" ) );
            }

            Util.Logger.Log( Util.LogLevel.Info, "Logging in" );
            var taskCompletionSource = new TaskCompletionSource<LoginResult>();

            Users.GetLoggedInUser().OnComplete(
                ( message ) =>
                {
                    if( message.IsError )
                    {
                        Util.Logger.Log( Util.LogLevel.Error, $"Could not find logged in user : {message.GetError().Message}" );
                        taskCompletionSource.TrySetResult( new LoginResult( ELoginResult.Failed, new UniqueUserId(), message.GetError().ToString() ) );
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Info, "Found logged in user" );
                        ulong userID = message.Data.ID;
                        uniquerUserID = new UniqueUserId( userID );
                        userOnlineAccount = new UserOnlineAccount( uniquerUserID );

                        //NOTE: Calling Users.GetLoggedInUser only returns the UserID,
                        //We must call Users.Get with the userID to get the user name

                        Users.Get( userID ).OnComplete(
                            ( message ) =>
                            {
                                string error_message = "";
                                ELoginResult result = ELoginResult.Failed;

                                if( message.IsError )
                                {
                                    error_message = message.GetError().Message;
                                    Util.Logger.Log( Util.LogLevel.Error, $"Could not find user : {error_message}" );
                                }
                                else
                                {
                                    Util.Logger.Log( Util.LogLevel.Info, "Found local user" );
                                    userOnlineAccount.SetUserAttributeByName( message.Data.DisplayName, UserOnlineAccount.DISPLAY_NAME );
                                    result = ELoginResult.SuccessLocalProfile;
                                    loginStatus = ELoginStatus.LoggedIn;
                                    OnLoginStatusChanged?.Invoke( ELoginStatus.NotLoggedIn, ELoginStatus.LoggedIn, GetUniqueUserId( 0 ) );
                                }

                                LoginResult loginResult = new LoginResult( result, uniquerUserID, error_message );
                                taskCompletionSource.TrySetResult( loginResult );
                            } );
                    }
                } );

            return taskCompletionSource.Task;
        }

        public Task<bool> Logout( int controller_id )
        {
            if( controller_id < 0
                && controller_id >= USAFUCore.Get().Platform.MaxLocalPlayers )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid Controller ID" );
                return Task.FromResult( false );
            }

            if( uniquerUserID == null )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Not logged in" );
                return Task.FromResult( false );
            }

            uniquerUserID = null;
            userOnlineAccount = null;

            loginStatus = ELoginStatus.NotLoggedIn;
            OnLoginStatusChanged?.Invoke( ELoginStatus.LoggedIn, ELoginStatus.NotLoggedIn, null );

            return Task.FromResult( true );
        }

        public IUserOnlineAccount GetUserAccount( IUniqueUserId user_id )
        {
            if( user_id == null
                || user_id != uniquerUserID )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid user ID" );
                return null;
            }

            return userOnlineAccount;
        }

        public IReadOnlyList<IUserOnlineAccount> GetAllUserAccounts()
        {
            if( userOnlineAccount != null )
            {
                return new ReadOnlyCollection<IUserOnlineAccount>( new List<IUserOnlineAccount> { userOnlineAccount } );
            }
            else
            {
                Util.Logger.Log( Util.LogLevel.Error, "Found no user account" );
                return null;
            }
        }

        public IUniqueUserId GetUniqueUserId( int controller_id )
        {
            if( controller_id < 0
                && controller_id >= USAFUCore.Get().Platform.MaxLocalPlayers )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid Controller ID" );
                return new UniqueUserId();
            }

            return uniquerUserID;
        }

        public IUniqueUserId CreateUniqueUserId( byte[] bytes, int size )
        {
            return uniquerUserID;
        }

        public IUniqueUserId CreateUniqueUserId( string textual_representation )
        {
            return uniquerUserID;
        }

        public ELoginStatus GetLoginStatus( IUniqueUserId user_id )
        {
            if( user_id == null
                || user_id != uniquerUserID )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid user ID" );
                return ELoginStatus.NotLoggedIn;
            }

            return GetLoginStatus( 0 );
        }

        public ELoginStatus GetLoginStatus( int controller_id )
        {
            if( controller_id < 0
                && controller_id >= USAFUCore.Get().Platform.MaxLocalPlayers )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid Controller ID" );
                return ELoginStatus.NotLoggedIn;
            }

            return loginStatus;
        }

        public string GetPlayerNickname( IUniqueUserId user_id )
        {
            if( user_id == null
                || user_id != uniquerUserID )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid user ID" );
                return "";
            }
            return userOnlineAccount.DisplayName;
        }

        public Task<EUserPrivilegesResult> GetUserPrivilege( IUniqueUserId user_id, EUserPrivileges privilege )
        {
            if( user_id == null
                || user_id != uniquerUserID )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid user ID" );
                return Task.FromResult( EUserPrivilegesResult.GenericFailure );
            }

            var taskCompletionSource = new TaskCompletionSource<EUserPrivilegesResult>();

            Entitlements.IsUserEntitledToApplication().OnComplete(
                ( message ) =>
                {
                    if( message.IsError )
                    {
                        Util.Logger.Log( Util.LogLevel.Error, "User is not entitled to the application !" );
                        taskCompletionSource.TrySetResult( EUserPrivilegesResult.UserNotFound );
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Info, "User is entitled to the application" );
                        taskCompletionSource.TrySetResult( EUserPrivilegesResult.NoFailures );
                    }
                } );

            return taskCompletionSource.Task; 
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
