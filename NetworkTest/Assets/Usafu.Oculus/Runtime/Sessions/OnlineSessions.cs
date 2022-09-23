using FishingCactus.Setup;
using FishingCactus.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oculus.Platform;
using Oculus.Platform.Models;

namespace FishingCactus.OnlineSessions
{
    public class OnlineSessioInfo : IOnlineSessionInfo
    {
        public bool IsValid => true;
        public string SessionId { get; private set; }

        public OnlineSessioInfo()
        {
            SessionId = Guid.NewGuid().ToString();
        }
    }

    public class OnlineSessions : IOnlineSessions
    {
#pragma warning disable CS0067 // The event 'event' is never used
        public event OnCreateSessionCompleteDelegate OnCreateSessionComplete;
        public event OnStartSessionCompleteDelegate OnStartSessionComplete;
        public event OnUpdateSessionCompleteDelegate OnUpdateSessionComplete;
        public event OnEndSessionCompleteDelegate OnEndSessionComplete;
        public event OnDestroySessionCompleteDelegate OnDestroySessionComplete;
        public event OnSessionFailureDelegate OnSessionFailure;
        public event OnJoinSessionCompleteDelegate OnJoinSessionComplete;
        public event OnSessionUserInviteAcceptedDelegate OnSessionUserInviteAccepted;
#pragma warning restore CS0067 // The event 'event' is never used

        private OnlineSessionState SessionState;
        private string CurrentSessionID;

        public void Initialize( Settings settings )
        {
            SessionState = OnlineSessionState.NoSession;
            CurrentSessionID = "";

            USAFUCore.Get().UserSystem.OnLoginStatusChanged += UserSystem_OnLoginStatusChanged; GroupPresence.SetJoinIntentReceivedNotificationCallback( OnJoinIntent );
        }

        private void UserSystem_OnLoginStatusChanged( ELoginStatus old_status, ELoginStatus new_status, IUniqueUserId new_user_id )
        {
            if( new_user_id == null
                || !new_user_id.IsValid
                )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Invalid user ID" );
            }

            if( new_status == ELoginStatus.LoggedIn )
            {
                GroupPresence.SetJoinIntentReceivedNotificationCallback( OnJoinIntent );
            }
        }

        public async Task<bool> CreateSession( IUniqueUserId user_id, string session_name, OnlineSessionSettings session_settings )
        {
            if( CanJoinSession( user_id, session_name ) )
            {
                Util.Logger.Log( Util.LogLevel.Info, "Creating session" );
                var options = new GroupPresenceOptions();
                options.SetIsJoinable( true );
                bool hasCreatedSession = await JoinGroupPresence( options, session_name );
                OnCreateSessionComplete?.Invoke( session_name, hasCreatedSession );
                return hasCreatedSession;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> JoinSession( IUniqueUserId user_id, string session_name, OnlineSessionSearchResult desired_session )
        {
            if( CanJoinSession( user_id, session_name ) )
            {
                Util.Logger.Log( Util.LogLevel.Info, "Joining session" );
                bool hasJoinedSession = await JoinGroupPresence( new GroupPresenceOptions(), session_name );
                JoinSessionCompleteResult result = hasJoinedSession ? JoinSessionCompleteResult.Success : JoinSessionCompleteResult.UnknownError;
                OnJoinSessionComplete?.Invoke( session_name, result );
                return hasJoinedSession;
            }
            else
            {
                return false;
            }
        }

        private Task<bool> JoinGroupPresence( GroupPresenceOptions options, string session_name )
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            options.SetDestinationApiName( USAFUCore.Get().Settings.Oculus.DestinationApiName );
            options.SetLobbySessionId( session_name );
            SessionState = OnlineSessionState.Pending;

            GroupPresence.Set( options ).OnComplete(
                ( message ) =>
                {
                    bool joinedSession = false;
                    if( !message.IsError )
                    {
                        Util.Logger.Log( Util.LogLevel.Info, "Set group presence" );
                        joinedSession = true;
                        SessionState = OnlineSessionState.InProgress;
                        CurrentSessionID = session_name;
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Error, $"Failed to set group presence. Error code : {message.GetError().Code}" );
                        SessionState = OnlineSessionState.NoSession;
                        CurrentSessionID = "";
                    }

                    taskCompletionSource.TrySetResult( joinedSession );
                } );

            return taskCompletionSource.Task;
        }

        private bool CanJoinSession( IUniqueUserId user_id, string session_name )
        {
            if( user_id == null
                || !user_id.IsValid
                )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Can't join session : Invalid User ID" );
                return false;
            }

            if( USAFUCore.Get().OnlineSessions == null )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Can't join session : Online sessions are not enabled" );
                return false;
            }

            if( CurrentSessionID == session_name )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Can't join session : Already in that session" );
                return false;
            }

            if( SessionState == OnlineSessionState.InProgress )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Can't join session : Already joining a session" );
                return false;
            }

            return true;
        }

        public Task<bool> EndSession( string session_name )
        {
            if( SessionState != OnlineSessionState.InProgress )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Not in a session" );
                return Task.FromResult( false );
            }

            Util.Logger.Log( Util.LogLevel.Info, "Leaving session" );
            var taskCompletionSource = new TaskCompletionSource<bool>();

            GroupPresence.Clear().OnComplete(
              ( message ) =>
              {
                  bool leftSession = false;
                  if( !message.IsError )
                  {
                      Util.Logger.Log( Util.LogLevel.Info, "Left session" );
                      leftSession = true;
                      SessionState = OnlineSessionState.NoSession;
                      CurrentSessionID = "";
                  }
                  else
                  {
                      Util.Logger.Log( Util.LogLevel.Error, $"Failed to leave session. Error : {message.GetError().Message}" );
                  }

                  taskCompletionSource.TrySetResult( leftSession );
                  OnEndSessionComplete?.Invoke( session_name, leftSession );
              } );

            return taskCompletionSource.Task;
        }

        public NamedOnlineSession GetNamedSession( string session_name )
        {
            if( session_name == CurrentSessionID )
            {
                return new NamedOnlineSession( session_name, new OnlineSessionSettings() );
            }
            else
            {
                return null;
            }
        }

        private void OnJoinIntent( Message<GroupPresenceJoinIntent> message )
        {
            Util.Logger.Log( Util.LogLevel.Info, "Found group presence join intent" );
            JoinSession( USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ), message.Data.LobbySessionId, new OnlineSessionSearchResult() );
        }


        public Task<bool> SendSessionInviteToFriends( IUniqueUserId user_id, IReadOnlyList<IUniqueUserId> friend_ids, string session_name )
        {
            return Task.FromResult( true );
        }

        public Task<bool> StartSession( string session_name )
        {
            return Task.FromResult( true );
        }

        public Task<bool> DestroySession( string session_name )
        {
            return Task.FromResult( true );
        }

        public Task<Tuple<bool, OnlineSessionSearchResult>> FindSessionById( IUniqueUserId user_id, string session_id, IUniqueUserId friend_id )
        {
            return Task.FromResult( new Tuple<bool, OnlineSessionSearchResult>( false, null ) );
        }

        public Task<Tuple<bool, OnlineSessionSearchResult>> FindFriendSession( IUniqueUserId user_id, IUniqueUserId friend_id )
        {
            return Task.FromResult( new Tuple<bool, OnlineSessionSearchResult>( false, null ) );
        }

        public Task<SessionInvitation> ConsumeSessionInvitation()
        {
            return Task.FromResult<SessionInvitation>( null );
        }
    }
}