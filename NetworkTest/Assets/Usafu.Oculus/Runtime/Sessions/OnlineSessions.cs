using FishingCactus.Setup;
using FishingCactus.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oculus.Platform;

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
        public void Initialize( Settings settings )
        {
        }

        public Task<bool> CreateSession( IUniqueUserId user_id, string session_name, OnlineSessionSettings session_settings )
        {
            Util.Logger.Log( Util.LogLevel.Info, "Creating session" );
            var options = new GroupPresenceOptions();
            options.SetIsJoinable( true );
            return JoinGroupPresence( options, session_name );
        }

        public Task<bool> JoinSession( IUniqueUserId user_id, string session_name, OnlineSessionSearchResult desired_session )
        {
            Util.Logger.Log( Util.LogLevel.Info, "Joining session" );
            return JoinGroupPresence( new GroupPresenceOptions(), session_name );
        }

        private Task<bool> JoinGroupPresence( GroupPresenceOptions options, string session_name )
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            options.SetDestinationApiName( USAFUCore.Get().Settings.Oculus.DestinationApiName );
            options.SetLobbySessionId( session_name );

            GroupPresence.Set( options ).OnComplete(
                ( message ) =>
                {
                    bool createdSession = false;
                    if( !message.IsError )
                    {
                        Util.Logger.Log( Util.LogLevel.Info, "Set group presence" );
                        createdSession = true;
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Error, $"Failed to set group presence. Error code : {message.GetError().Code}" );
                    }

                    taskCompletionSource.TrySetResult( createdSession );
                } );

            return taskCompletionSource.Task;
        }

        public Task<bool> EndSession( string session_name )
        {
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

        public Task<bool> SendSessionInviteToFriends( IUniqueUserId user_id, IReadOnlyList<IUniqueUserId> friend_ids, string session_name )
        {
            Util.Logger.Log( Util.LogLevel.Info, "Sending invite to friends" );
            USAFUCore.Get().ExternalUI.ShowInviteUI( USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ), session_name );
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

        public NamedOnlineSession GetNamedSession( string session_name )
        {
            return null;
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