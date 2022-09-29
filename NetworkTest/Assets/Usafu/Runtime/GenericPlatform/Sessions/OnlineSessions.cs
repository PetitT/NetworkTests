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
        public event OnMatchmakingCompleteDelegate OnMatchmakingComplete;
        public event OnMatchmakingCancelCompleteDelegate OnMatchmakingCancelComplete;
#pragma warning restore CS0067 // The event 'event' is never used
        public void Initialize( Settings settings )
        {
            sessionMap.Clear();
        }

        public Task< bool > CreateSession( IUniqueUserId user_id, string session_name, OnlineSessionSettings session_settings )
        {
            if ( sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            var named_online_session = new NamedOnlineSession( session_name, session_settings );
            named_online_session.SessionInfo = new OnlineSessioInfo();

            sessionMap.Add( session_name, named_online_session );

            return Task.FromResult( true );
        }

        public Task< bool > StartSession( string session_name )
        {
            if ( !sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }

        public Task<bool> EndSession( string session_name )
        {
            if ( sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }

        public Task< bool > DestroySession( string session_name )
        {
            if ( !sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            sessionMap.Remove( session_name );
            return Task.FromResult( true );
        }

        public Task<bool> JoinSession( IUniqueUserId user_id, string session_name, OnlineSessionSearchResult desired_session )
        {
            if( !sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }

        public Task<bool> UpdateSession(string session_name, OnlineSessionSettings new_session_settings )
        {
            if( !sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }

        public Task<bool> StartMatchmaking( List<IUniqueUserId> user_ids, string session_name, OnlineSessionSettings new_session_settings )
        {
            return Task.FromResult( true );
        }

        public Task<bool> CancelMatchmaking( IUniqueUserId user_id, string session_name )
        {
            return Task.FromResult( true );
        }

        public NamedOnlineSession GetNamedSession( string session_name )
        {
            if ( sessionMap.TryGetValue( session_name, out NamedOnlineSession named_online_session ) )
            {
                return named_online_session;
            }

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
            return Task.FromResult( true );
        }

        public Task< SessionInvitation > ConsumeSessionInvitation()
        { 
            return Task.FromResult< SessionInvitation >( null );
        }

        private readonly Dictionary< string, NamedOnlineSession > sessionMap = new Dictionary<string, NamedOnlineSession>();
    }
}