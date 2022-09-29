using FishingCactus.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingCactus.OnlineSessions
{
    public enum OnlineSessionState
    {
        NoSession,
        Creating,
        Pending,
        Starting,
        InProgress,
        Ending,
        Ended,
        Destroying,
        Destroyed
    }

    public enum SessionFailure
    {
        ServiceConnectionLost
    }

    public enum OnlineDataAdvertisementType
    {
        /** Don't advertise via the online service or QoS data */
        DontAdvertise,
        /** Advertise via the server ping data only */
        ViaPingOnly,
        /** Advertise via the online service only */
        ViaOnlineService,
        /** Advertise via the online service and via the ping data */
        ViaOnlineServiceAndPing
    }

    public struct OnlineSessionSetting
    {
        public string Data;
        public OnlineDataAdvertisementType AdvertisementType;
        /** Optional ID used in some platforms as the index instead of the session name */
        public int ID;
    }

    public class OnlineSessionSettings
    {
        public Dictionary<string, OnlineSessionSetting> Settings = new Dictionary<string, OnlineSessionSetting>();
        public int NumPublicConnections = 0;
        public int NumPrivateConnections = 0;
        public bool ShouldAdvertise = true;
        public bool AllowJoinInProgress = true;
        public bool IsDedicated = false;
        public bool UsesStats = false;
        public bool IsLANMatch = false;
        public bool AllowInvites = true;
        public bool UsesPresence = true;
        public bool AllowJoinViaPresence = true;
        public bool AllowJoinViaPresenceFriendsOnly = false;
        public bool AntiCheatProtected = false;
    }

    public interface IOnlineSessionInfo
    {
        bool IsValid { get; }
        string SessionId { get; }
    }

    public class OnlineSession
    {
        public IUniqueUserId OwningUserId { get; set; }
        public string OwningUserName { get; set; }
        public OnlineSessionSettings SessionSettings { get; set; }
        public int NumberOpenPrivateConnections { get; set; }
        public int NumberOpenPublicConnections { get; set; }
        public IOnlineSessionInfo SessionInfo { get; set; }
        public string SessionIdStr => SessionInfo != null && SessionInfo.IsValid ? SessionInfo.SessionId.ToString() : "InvalidSession";

        public OnlineSession()
        {
            NumberOpenPrivateConnections = 0;
            NumberOpenPublicConnections = 0;
            SessionSettings = new OnlineSessionSettings();
        }

        public OnlineSession( OnlineSessionSettings session_settings )
            : this()
        {
            SessionSettings = session_settings;
        }

        public OnlineSession( OnlineSession other_session )
        {
            OwningUserId = other_session.OwningUserId;
            OwningUserName = other_session.OwningUserName;
            SessionSettings = other_session.SessionSettings;
            NumberOpenPrivateConnections = other_session.NumberOpenPrivateConnections;
            NumberOpenPublicConnections = other_session.NumberOpenPublicConnections;
            SessionInfo = other_session.SessionInfo;
        }
    }

    public class OnlineSessionSearchResult
    {
        public OnlineSession Session { get; set; }
        public int PingInMs { get; set; }
        public bool IsValid => Session.OwningUserId != null && Session.OwningUserId.IsValid && IsSessionInfoValid;
        public bool IsSessionInfoValid => Session.SessionInfo != null && Session.SessionInfo.IsValid;
        public string SessionIdStr => Session.SessionIdStr;

        public OnlineSessionSearchResult()
        {
            Session = new OnlineSession();
        }

        public override string ToString()
        {
            return $"Session Id : {SessionIdStr} - IsValid {IsValid} - IsSessionInfoValid {IsSessionInfoValid}";
        }
    };

    public enum JoinSessionCompleteResult
    {
        /** The join worked as expected */
        Success,
        /** There are no open slots to join */
        SessionIsFull,
        /** The session couldn't be found on the service */
        SessionDoesNotExist,
        /** There was an error getting the session server's address */
        CouldNotRetrieveAddress,
        /** The user attempting to join is already a member of the session */
        AlreadyInSession,
        /** An error not covered above occurred */
        UnknownError
    }

    public class NamedOnlineSession : OnlineSession
    {
        public string SessionName;
        public bool IsHosting = false;
        public IUniqueUserId LocalOwnerId;
        public OnlineSessionState SessionState = OnlineSessionState.NoSession;

        public NamedOnlineSession( string session_name, OnlineSessionSettings session_settings )
            : base( session_settings )
        {
            SessionName = session_name;
        }

        public NamedOnlineSession( string session_name, OnlineSession session )
            : base( session )
        {
            SessionName = session_name;
        }
    }

    public delegate void OnCreateSessionCompleteDelegate( string session_name, bool success );
    public delegate void OnStartSessionCompleteDelegate( string session_name, bool success );
    public delegate void OnUpdateSessionCompleteDelegate( string session_name, bool success );
    public delegate void OnEndSessionCompleteDelegate( string session_name, bool success );
    public delegate void OnDestroySessionCompleteDelegate( string session_name, bool success );
    public delegate void OnSessionFailureDelegate( IUniqueUserId user_id, SessionFailure session_failure );
    public delegate void OnJoinSessionCompleteDelegate( string session_name, JoinSessionCompleteResult result );
    public delegate void OnSessionUserInviteAcceptedDelegate( bool success, IUniqueUserId user_id, OnlineSessionSearchResult session_search_result );
    public delegate void OnMatchmakingCompleteDelegate( string session_name, bool success );
    public delegate void OnMatchmakingCancelCompleteDelegate( string session_name, bool success );

    public class SessionInvitation
    {
        public IUniqueUserId InvitedUserId;
        public OnlineSessionSearchResult SessionSearchResult;
    };

    public interface IOnlineSessions
    {
        event OnCreateSessionCompleteDelegate OnCreateSessionComplete;
        event OnStartSessionCompleteDelegate OnStartSessionComplete;
        event OnUpdateSessionCompleteDelegate OnUpdateSessionComplete;
        event OnEndSessionCompleteDelegate OnEndSessionComplete;
        event OnDestroySessionCompleteDelegate OnDestroySessionComplete;
        event OnSessionFailureDelegate OnSessionFailure;
        event OnJoinSessionCompleteDelegate OnJoinSessionComplete;
        event OnSessionUserInviteAcceptedDelegate OnSessionUserInviteAccepted;
        event OnMatchmakingCompleteDelegate OnMatchmakingComplete;
        event OnMatchmakingCancelCompleteDelegate OnMatchmakingCancelComplete;

        void Initialize( Setup.Settings settings );
        Task<bool> CreateSession( IUniqueUserId user_id, string session_name, OnlineSessionSettings session_settings );
        Task<bool> StartSession( string session_name );
        Task<bool> EndSession( string session_name );
        Task<bool> DestroySession( string session_name );
        Task<bool> JoinSession( IUniqueUserId user_id, string session_name, OnlineSessionSearchResult desired_session );
        Task<bool> UpdateSession( string session_name, OnlineSessionSettings new_session_settings );
        Task<bool> StartMatchmaking( List<IUniqueUserId> user_ids, string session_name, OnlineSessionSettings new_session_settings );
        Task<bool> CancelMatchmaking( IUniqueUserId user_id, string session_name );
        NamedOnlineSession GetNamedSession( string session_name );
        Task<Tuple<bool, OnlineSessionSearchResult>> FindSessionById( IUniqueUserId user_id, string session_id, IUniqueUserId friend_id );
        Task<Tuple<bool, OnlineSessionSearchResult>> FindFriendSession( IUniqueUserId user_id, IUniqueUserId friend_id );
        Task< bool > SendSessionInviteToFriends( IUniqueUserId user_id, IReadOnlyList< IUniqueUserId > friend_ids, string session_name );
        Task< SessionInvitation > ConsumeSessionInvitation();
    }
}