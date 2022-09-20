using FishingCactus.Setup;
using FishingCactus.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var taskCompletionSource = new TaskCompletionSource<bool>();

            var options = new Oculus.Platform.GroupPresenceOptions();
            options.SetIsJoinable(session_settings.AllowJoinViaPresence);
            options.SetDestinationApiName("game_lobby");
            options.SetLobbySessionId(session_name);
            options.SetMatchSessionId("Lobby");
            Oculus.Platform.GroupPresence.Set(options).OnComplete(
                (message) =>
                {
                    bool createdSession = false;
                    if (!message.IsError)
                    {
                        createdSession = true;
                    }

                    OnCreateSessionComplete?.Invoke(session_name, createdSession);
                    taskCompletionSource.TrySetResult(createdSession);
                });

            return taskCompletionSource.Task;
        }

        public Task<bool> StartSession( string session_name )
        {
            return Task.FromResult( true );
        }

        public Task<bool> EndSession( string session_name )
        {
            Oculus.Platform.GroupPresence.Clear();
            OnEndSessionComplete?.Invoke(session_name, true);
            return Task.FromResult( true );
        }

        public Task<bool> DestroySession( string session_name )
        {
            return Task.FromResult( true );
        }

        public Task<bool> JoinSession( IUniqueUserId user_id, string session_name, OnlineSessionSearchResult desired_session )
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

        public Task<bool> SendSessionInviteToFriends( IUniqueUserId user_id, IReadOnlyList<IUniqueUserId> friend_ids, string session_name )
        {
            USAFUCore.Get().ExternalUI.ShowInviteUI( USAFUCore.Get().UserSystem.GetUniqueUserId(0), session_name );

            return Task.FromResult( true );
        }

        public Task<SessionInvitation> ConsumeSessionInvitation()
        { 
            return Task.FromResult< SessionInvitation >( null );
        }
    }
}