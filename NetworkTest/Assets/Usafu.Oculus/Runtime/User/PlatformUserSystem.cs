using FishingCactus.Setup;
using Oculus.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class PlatformUserSystem : IPlatformUserSystem
    {
        private UserOnlineAccount userOnlineAccount;
        private UniqueUserId uniquerUserID;

        public Task<LoginResult> Login( int controller_id )
        {
            Util.Logger.Log(Util.LogLevel.Info, "Logging in");
            var taskCompletionSource = new TaskCompletionSource<LoginResult>();

            Oculus.Platform.Users.GetLoggedInUser().OnComplete(
                ( message ) =>
                {
                    if ( message.IsError )
                    {
                        Util.Logger.Log( Util.LogLevel.Warning, "Could not find logged in user" );
                        uniquerUserID = new UniqueUserId();
                        userOnlineAccount = new UserOnlineAccount( uniquerUserID );
                        LoginResult result = new LoginResult( ELoginResult.Failed, uniquerUserID, message.GetError().ToString() );
                        taskCompletionSource.TrySetResult( result );
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Info, "Found logged in user" );
                        ulong userID = message.Data.ID;
                        uniquerUserID = new UniqueUserId( userID );
                        //NOTE: Calling Users.GetLoggedInUser only returns the UserID,
                        //We must call Users.Get with the userID to get all user data
                        LoginResult result = new LoginResult(ELoginResult.SuccessOnlineProfile, uniquerUserID, "");
                        taskCompletionSource.TrySetResult(result);

                        //Oculus.Platform.Users.Get(userID).OnComplete(
                        //    (message) =>
                        //    {
                        //        string player_name = "";
                        //        string error_message = "";
                        //        ELoginResult result = ELoginResult.Failed;

                        //        if (!message.IsError)
                        //        {
                        //            Util.Logger.Log(Util.LogLevel.Info, "Found local user");
                        //            player_name = message.Data.DisplayName;
                        //            result = ELoginResult.SuccessLocalProfile;
                        //        }
                        //        else
                        //        {
                        //            Util.Logger.Log(Util.LogLevel.Warning, "Could not find user");
                        //            error_message = message.GetError().ToString();
                        //        }

                        //        userOnlineAccount = new UserOnlineAccount(uniquerUserID, player_name);
                        //        LoginResult loginResult = new LoginResult(result, uniquerUserID, error_message);
                        //        taskCompletionSource.TrySetResult( loginResult );
                        //    });
                    }
                });

            return taskCompletionSource.Task; 
        }

        public Task< bool > Logout( int controller_id )
        {
            return Task.FromResult( true );
        }

        public IUserOnlineAccount GetUserAccount( IUniqueUserId user_id )
        {
            return userOnlineAccount;
        }

        public IReadOnlyList< IUserOnlineAccount > GetAllUserAccounts()
        {
            return new ReadOnlyCollection< IUserOnlineAccount >( new List< IUserOnlineAccount > { userOnlineAccount } );
        }

        public IUniqueUserId GetUniqueUserId( int controller_id )
        {
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
            return GetLoginStatus( 0 );
        }
        
        public ELoginStatus GetLoginStatus( int controller_id )
        {
            return ELoginStatus.UsingLocalProfile;
        }

        public string GetPlayerNickname( IUniqueUserId user_id )
        {
            return userOnlineAccount.DisplayName;
        }

        public Task< EUserPrivilegesResult > GetUserPrivilege( IUniqueUserId user_id, EUserPrivileges privilege )
        {
            return Task.FromResult( EUserPrivilegesResult.NoFailures );
        }

        public void Initialize( Settings settings )
        {
            
        }

        public Task<GetUserAvatarResult> GetUserAvatar(IUniqueUserId user_id, AvatarSize avatar_size)
        {
            return Task.FromResult(new GetUserAvatarResult { Success = false, Avatar = null });
        }

#pragma warning disable CS0067 // The event 'event' is never used
        public event OnControllerPairingChangedDelegate OnControllerPairingChanged;
        public event OnLoginStatusChangedDelegate OnLoginStatusChanged;
#pragma warning restore CS0067 // The event 'event' is never used
    }
}
