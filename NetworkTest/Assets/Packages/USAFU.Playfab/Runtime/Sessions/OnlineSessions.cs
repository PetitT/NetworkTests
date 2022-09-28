using FishingCactus.Setup;
using FishingCactus.User;
using PlayFab.MultiplayerModels;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FishingCactus.Util.Logger;
using static HelperFunctions;
using System.Threading;
using PlayFab.Multiplayer;

namespace FishingCactus.OnlineSessions
{
    public class OnlineSessionInfo : IOnlineSessionInfo
    {
        public bool IsValid => Lobby != null;
        public int NumPlayers = 0;
        public string SessionId => Lobby.Id;
        public string ConnectionString => Lobby.ConnectionString;

        public PlayFab.Multiplayer.Lobby Lobby { get; set; }
        public OnlineSessionInfo( PlayFab.Multiplayer.Lobby lobby )
        {
            Lobby = lobby;
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

        private const float TimeBetweenSessionsCheck = 6f;
        private PlayFab.MultiplayerModels.EntityKey MultiplayerKey;

        private PFEntityKey EntityKey;

        private Dictionary<string, NamedOnlineSession> SessionMap = new Dictionary<string, NamedOnlineSession>();
        private NamedOnlineSession CurrentSession;
        private OnlineSessionState CurrentSessionState;

        private TaskCompletionSource<bool> LobbyCreatedTask;
        private TaskCompletionSource<bool> LobbyJoinedTask;
        private TaskCompletionSource<bool> LobbyLeaveTask;
        private TaskCompletionSource<bool> LobbyUpdateTask;

        public void Initialize( Settings settings )
        {
            SessionMap.Clear();
            CurrentSessionState = OnlineSessionState.NoSession;
            PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted += OnLobbyCreatedAndJoined;
            PlayFabMultiplayer.OnLobbyJoinCompleted += OnLobbyJoinCompleted;
            PlayFabMultiplayer.OnLobbyLeaveCompleted += OnLobbyLeaveCompleted;
            PlayFabMultiplayer.OnLobbyUpdated += OnLobbyUpdated;
            PlayFabMultiplayer.OnMatchmakingTicketCompleted += OnMatchmakingTicketCompleted;
            PlayFabMultiplayer.OnMatchmakingTicketStatusChanged += OnMatchmakingTicketStatusChanged;
            PlayFabMultiplayer.OnError += PlayFabMultiplayer_OnError;
        }

        private void PlayFabMultiplayer_OnError( PlayFabMultiplayerErrorArgs args )
        {
            Log( Util.LogLevel.Error, args.Message );
        }

        public Task<bool> CreateSession( IUniqueUserId user_id, string session_name, OnlineSessionSettings session_settings )
        {
            if( !CanJoinSession( session_name, user_id ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Creating a lobby" );
            CurrentSessionState = OnlineSessionState.Creating;
            LobbyCreatedTask = new TaskCompletionSource<bool>();

            var lobby_create_config = new LobbyCreateConfiguration
            {
                MaxMemberCount = ( uint )session_settings.NumPublicConnections,
                OwnerMigrationPolicy = LobbyOwnerMigrationPolicy.Automatic,
                LobbyProperties = new Dictionary<string, string>() { { StringConstants.SESSION_NAME, session_name } }
            };

            var lobby_join_config = new LobbyJoinConfiguration { };

            PlayFabMultiplayer.CreateAndJoinLobby(
                GetPlayerEntityKey(),
                lobby_create_config,
                lobby_join_config
                );

            return LobbyCreatedTask.Task;
        }

        private void OnLobbyCreatedAndJoined( PlayFab.Multiplayer.Lobby lobby, int result )
        {
            if( result == 0 )
            {
                string session_name = lobby.GetLobbyProperties()[StringConstants.SESSION_NAME];
                Log( Util.LogLevel.Info, $"Created lobby : {session_name}" );
                CurrentSession = AddNamedSession( lobby );

                CurrentSession.SessionSettings.Settings.Add(
                    StringConstants.CONNEXION_STRING,
                    new OnlineSessionSetting { Data = lobby.ConnectionString }
                    );

                CurrentSessionState = OnlineSessionState.InProgress;
                OnCreateSessionComplete?.Invoke( session_name, true );
                LobbyCreatedTask.TrySetResult( true );
            }
            else
            {
                Log( Util.LogLevel.Error, $"Couldn't create lobby : {result}" );
                CurrentSessionState = OnlineSessionState.NoSession;
                OnCreateSessionComplete?.Invoke( "", false );
                LobbyCreatedTask.TrySetResult( false );
            }
        }

        public Task<bool> JoinSession( IUniqueUserId user_id, string session_name, OnlineSessionSearchResult desired_session )
        {
            if( !CanJoinSession( session_name, user_id ) )
            {
                return Task.FromResult( false );
            }

            CurrentSessionState = OnlineSessionState.Pending;
            LobbyJoinedTask = new TaskCompletionSource<bool>();
            string connexion_string = desired_session.Session.SessionSettings.Settings[StringConstants.CONNEXION_STRING].Data;
            var member_properties = new Dictionary<string, string>();

            Log( Util.LogLevel.Info, $"Joining lobby : {connexion_string}" );

            PlayFabMultiplayer.JoinLobby(
                GetPlayerEntityKey(),
                connexion_string,
                member_properties
                );

            return LobbyJoinedTask.Task;
        }

        private void OnLobbyJoinCompleted( PlayFab.Multiplayer.Lobby lobby, PFEntityKey newMember, int result )
        {
            if( result == 0 )
            {
                Log( Util.LogLevel.Info, "Joined lobby" );
                string session_name = lobby.GetLobbyProperties()[StringConstants.SESSION_NAME];
                CurrentSessionState = OnlineSessionState.InProgress;
                CurrentSession = AddNamedSession( lobby );
                OnJoinSessionComplete?.Invoke( session_name, JoinSessionCompleteResult.Success );
                LobbyJoinedTask.TrySetResult( true );
            }
            else
            {
                Log( Util.LogLevel.Error, $"Couldn't join lobby : {result}" );
                CurrentSessionState = OnlineSessionState.NoSession;
                OnJoinSessionComplete?.Invoke( "", JoinSessionCompleteResult.UnknownError );
                LobbyJoinedTask.TrySetResult( false );
            }
        }

        public Task<bool> EndSession( string session_name )
        {
            if( !CanLeaveSession( session_name ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Leaving Lobby" );
            CurrentSessionState = OnlineSessionState.Ending;
            LobbyLeaveTask = new TaskCompletionSource<bool>();

            GetOnlineSessionInfo().Lobby.Leave( GetPlayerEntityKey() );
            return LobbyLeaveTask.Task;
        }

        private void OnLobbyLeaveCompleted( PlayFab.Multiplayer.Lobby lobby, PFEntityKey localUser )
        {
            Log( Util.LogLevel.Info, "Left Lobby" );
            string session_name = lobby.GetLobbyProperties()[StringConstants.SESSION_NAME];
            RemoveNamedSession( session_name );
            CurrentSession = null;
            CurrentSessionState = OnlineSessionState.NoSession;
            OnEndSessionComplete?.Invoke( session_name, true );
            LobbyLeaveTask.TrySetResult( true );
        }

        public Task<bool> UpdateSession( string session_name, OnlineSessionSettings updated_session_settings )
        {
            if( GetNamedSession( session_name ) == null )
            {
                Log( Util.LogLevel.Warning, "Can't update session : session does not exist" );
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Updating lobby data" );
            LobbyUpdateTask = new TaskCompletionSource<bool>();
            var new_properties = new Dictionary<string, string>();

            foreach( var item in GetOnlineSessionInfo().Lobby.GetLobbyProperties() )
            {
                new_properties.Add( item.Key, item.Value );
            }

            foreach( var item in updated_session_settings.Settings )
            {
                if( new_properties.ContainsKey( item.Key ) )
                {
                    new_properties[item.Key] = item.Value.Data;
                }
                else
                {
                    new_properties.Add( item.Key, item.Value.Data );
                }
            }

            var lobby_update = new LobbyDataUpdate
            {
                LobbyProperties = new_properties
            };

            GetOnlineSessionInfo().Lobby.PostUpdate(
               GetPlayerEntityKey(),
               lobby_update
                );

            return LobbyUpdateTask.Task;
        }

        private void OnLobbyUpdated( PlayFab.Multiplayer.Lobby lobby, bool ownerUpdated, bool maxMembersUpdated, bool accessPolicyUpdated, bool membershipLockUpdated, IList<string> updatedSearchPropertyKeys, IList<string> updatedLobbyPropertyKeys, IList<LobbyMemberUpdateSummary> memberUpdates )
        {
            if( CurrentSession == null ) { return; }
            Log( Util.LogLevel.Info, $"Lobby properties updated" );
            GetOnlineSessionInfo().Lobby = lobby;
            CurrentSession.SessionSettings.Settings = GetSettingsFromDictionnary( lobby.GetLobbyProperties() );
            OnUpdateSessionComplete?.Invoke( lobby.GetLobbyProperties()[StringConstants.SESSION_NAME], true );
            LobbyUpdateTask?.TrySetResult( true );
        }

        public Task<bool> StartSession( string session_name )
        {
            if( !SessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }

        public Task<bool> DestroySession( string session_name )
        {
            if( SessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }


        public Task<bool> StartMatchmaking( List<IUniqueUserId> user_ids, string session_name, OnlineSessionSettings new_session_settings )
        {
            //Need to add "Latencies", otherwise it just breaks
            //string attributes = "\"{\"elo\":\"50\"}\"";
            MatchUser localUser = new MatchUser( GetPlayerEntityKey(), "" );
            uint matchmaking_time = Convert.ToUInt32( new_session_settings.Settings[StringConstants.MATCHMAKING_TIME].Data );

            PlayFabMultiplayer.CreateMatchmakingTicket(
                localUser,
                session_name,
                matchmaking_time
                );

            return Task.FromResult( true );
        }

        private void OnMatchmakingTicketCompleted( MatchmakingTicket ticket, int result )
        {
            Log( Util.LogLevel.Info, "Matchmaking ticket completed" );
        }


        private void OnMatchmakingTicketStatusChanged( MatchmakingTicket ticket )
        {
            Log( Util.LogLevel.Info, $"Status changed : {ticket.Status}" );
        }

        private NamedOnlineSession AddNamedSession( PlayFab.Multiplayer.Lobby lobby )
        {
            string session_name = lobby.GetLobbyProperties()[StringConstants.SESSION_NAME];
            NamedOnlineSession newSession = new NamedOnlineSession(
                 session_name,
                 new OnlineSession
                 {
                     SessionInfo = new OnlineSessionInfo( lobby )
                     {
                         Lobby = lobby,
                     }
                 } );

            SessionMap.Add( session_name, newSession );
            return newSession;
        }

        private void RemoveNamedSession( string session_name )
        {
            SessionMap.Remove( session_name );
        }

        public NamedOnlineSession GetNamedSession( string session_name )
        {
            if( SessionMap.TryGetValue( session_name, out NamedOnlineSession named_online_session ) )
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

        public Task<SessionInvitation> ConsumeSessionInvitation()
        {
            return Task.FromResult<SessionInvitation>( null );
        }

        private bool CanJoinSession( string session_name, IUniqueUserId user_id )
        {
            if( !IsUserValid( user_id ) )
            {
                Log( Util.LogLevel.Warning, "Can't join session : Invalid user id" );
                return false;
            }

            if( CurrentSession != null )
            {
                Log( Util.LogLevel.Warning, "Can't join session : Already in a session" );
                return false;
            }

            if( GetNamedSession( session_name ) != null )
            {
                Log( Util.LogLevel.Warning, "Can't join session : Session already exists" );
                return false;
            }

            if( CurrentSessionState != OnlineSessionState.NoSession )
            {
                Log( Util.LogLevel.Warning, "Can't join a session right now" );
                return false;
            }

            return true;
        }

        private bool CanLeaveSession( string session_name )
        {
            if( CurrentSession == null )
            {
                Log( Util.LogLevel.Warning, "Can't leave session : not in a session" );
                return false;
            }

            if( GetNamedSession( session_name ) == null )
            {
                Log( Util.LogLevel.Warning, "Can't leave session : session does not exist" );
                return false;
            }

            OnlineSessionInfo info = GetNamedSession( session_name ).SessionInfo as OnlineSessionInfo;

            if( !info.IsValid )
            {
                Log( Util.LogLevel.Warning, "Can't leave session : invalid lobby" );
            }

            if( CurrentSessionState != OnlineSessionState.InProgress )
            {
                Log( Util.LogLevel.Warning, "Can't leave session right now" );
                return false;
            }

            return true;
        }

        private PFEntityKey GetPlayerEntityKey()
        {
            if( EntityKey == null )
            {
                USAFUCore core = USAFUCore.Get();
                IUniqueUserId userID = core.UserSystem.GetUniqueUserId( 0 );
                IUserOnlineAccount account = core.UserSystem.GetUserAccount( userID );
                account.GetAuthAttributeByName( out string ID, StringConstants.ENTITY_ID );
                account.GetAuthAttributeByName( out string entity_type, StringConstants.ENTITY_TYPE );
                EntityKey = new PFEntityKey( ID, entity_type );
            }

            return EntityKey;
        }

        private OnlineSessionInfo GetOnlineSessionInfo()
        {
            if( CurrentSession == null )
            {
                return null;
            }

            return CurrentSession.SessionInfo as OnlineSessionInfo;
        }

        private Dictionary<string, OnlineSessionSetting> GetSettingsFromDictionnary( IDictionary<string, string> datas )
        {
            var settings = new Dictionary<string, OnlineSessionSetting>();
            foreach( var item in datas )
            {
                settings.Add( item.Key, new OnlineSessionSetting { Data = item.Value } );
            }

            return settings;
        }

    }
}