using FishingCactus.Setup;
using FishingCactus.User;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FishingCactus.Util.Logger;
using static HelperFunctions;
using System.Threading;
using PlayFab.Multiplayer;
using System.Linq;

namespace FishingCactus.OnlineSessions
{
    public class OnlineSessionInfo : IOnlineSessionInfo
    {
        public bool IsValid => Lobby != null;
        public string SessionId => Lobby.Id;

        public Lobby Lobby { get; set; }
        public OnlineSessionInfo( Lobby lobby )
        {
            Lobby = lobby;
        }
    }

    public class MatchmakingInfo
    {
        public string QueueName;
        public MatchmakingTicket Ticket;
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

        private PFEntityKey EntityKey;

        private Dictionary<string, NamedOnlineSession> SessionMap = new Dictionary<string, NamedOnlineSession>();
        private MatchmakingInfo CurrentMatchmakingInfo;
        private NamedOnlineSession CurrentSession;
        private OnlineSessionState CurrentSessionState;

        private TaskCompletionSource<bool> LobbyCreatedTask;
        private TaskCompletionSource<bool> LobbyJoinedTask;
        private TaskCompletionSource<bool> LobbyLeaveTask;
        private TaskCompletionSource<bool> LobbyUpdateTask;
        private TaskCompletionSource<bool> MatchmakingBeginTask;
        private TaskCompletionSource<bool> MatchmakingCancelTask;

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

        private void OnLobbyCreatedAndJoined( Lobby lobby, int result )
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
            if( !CanJoinSession( session_name, user_id, desired_session ) )
            {
                return Task.FromResult( false );
            }

            CurrentSessionState = OnlineSessionState.Pending;
            LobbyJoinedTask = new TaskCompletionSource<bool>();

            var connexion_string = desired_session.Session.SessionSettings.Settings[StringConstants.CONNEXION_STRING].Data;
            var member_properties = new Dictionary<string, string>();

            Log( Util.LogLevel.Info, $"Joining lobby : {connexion_string}" );

            PlayFabMultiplayer.JoinLobby(
                GetPlayerEntityKey(),
                connexion_string,
                member_properties
                );

            return LobbyJoinedTask.Task;
        }

        private void OnLobbyJoinCompleted( Lobby lobby, PFEntityKey new_member, int result )
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

        private void OnLobbyLeaveCompleted( Lobby lobby, PFEntityKey local_User )
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

        private void OnLobbyUpdated( Lobby lobby, bool ownerUpdated, bool maxMembersUpdated, bool accessPolicyUpdated, bool membershipLockUpdated, IList<string> updatedSearchPropertyKeys, IList<string> updatedLobbyPropertyKeys, IList<LobbyMemberUpdateSummary> memberUpdates )
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
            if( !CanJoinMatchmakingQueue( user_ids[0], new_session_settings ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Joining Matchmaking queue" );
            MatchmakingBeginTask = new TaskCompletionSource<bool>();

            string attributes = "";
            if( new_session_settings.Settings.TryGetValue( StringConstants.MATCHMAKING_ATTRIBUTES, out OnlineSessionSetting setting ) )
            {
                attributes = setting.Data;
            }

            var matchmaking_time = Convert.ToUInt32( new_session_settings.Settings[StringConstants.MATCHMAKING_TIME].Data );
            var local_User = new MatchUser( GetPlayerEntityKey(), attributes );

            MatchmakingTicket ticket = PlayFabMultiplayer.CreateMatchmakingTicket(
                  local_User,
                  session_name,
                  matchmaking_time
                  );

            CurrentMatchmakingInfo = new MatchmakingInfo
            {
                Ticket = ticket,
                QueueName = session_name
            };

            return MatchmakingBeginTask.Task;
        }

        public Task<bool> CancelMatchmaking( IUniqueUserId user_id, string session_name )
        {
            if( !CanLeaveMatchmakingQueue( user_id ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Canceling matchmaking ticket" );
            MatchmakingCancelTask = new TaskCompletionSource<bool>();
            CurrentMatchmakingInfo.Ticket.Cancel();
            return MatchmakingCancelTask.Task;
        }

        private void OnMatchmakingTicketStatusChanged( MatchmakingTicket ticket )
        {
            Log( Util.LogLevel.Info, $"Matchmaking Status changed : {ticket.Status}" );

            if( ticket.Status == MatchmakingTicketStatus.WaitingForMatch )
            {
                Log( Util.LogLevel.Info, "Succesfully joined matchmaking" );
                MatchmakingBeginTask?.TrySetResult( true );
            }

            if( ticket.Status == MatchmakingTicketStatus.Failed )
            {
                Log( Util.LogLevel.Warning, "Matchmaking ticket failed to find a match" );
                MatchmakingBeginTask?.TrySetResult( false );
            }

            if( ticket.Status == MatchmakingTicketStatus.Canceled )
            {
                Log( Util.LogLevel.Info, "Matchmaking ticket was canceled" );
                MatchmakingCancelTask?.TrySetResult( true );
                MatchmakingBeginTask?.TrySetResult( false );
                OnMatchmakingCancelComplete?.Invoke( CurrentMatchmakingInfo.QueueName, true );
            }

            if( ticket.Status == MatchmakingTicketStatus.Matched )
            {
                Log( Util.LogLevel.Info, "Matchmaking ticket found a match" );
                MatchmakingCancelTask?.TrySetResult( false );
            }
        }

        private void OnMatchmakingTicketCompleted( MatchmakingTicket ticket, int result )
        {
            bool success = result == 0;

            if( success )
            {
                Log( Util.LogLevel.Info, $"Matchmaking ticket succesfully found a match" );
            }
            else
            {
                Log( Util.LogLevel.Warning, "Matchmaking ticket failed to find a match" );
            }

            OnMatchmakingComplete?.Invoke( CurrentMatchmakingInfo.QueueName, success );
            CurrentMatchmakingInfo = null;
        }

        private NamedOnlineSession AddNamedSession( Lobby lobby )
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

        private bool CanJoinSession( string session_name, IUniqueUserId user_id, OnlineSessionSearchResult desired_session = null )
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

            if( desired_session != null )
            {
                if( !desired_session.Session.SessionSettings.Settings.ContainsKey( StringConstants.CONNEXION_STRING ) )
                {
                    Log( Util.LogLevel.Warning, $"Can't join session : Session settings must contain a {StringConstants.CONNEXION_STRING} key" );
                    return false;
                }
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
                return false;
            }

            if( CurrentSessionState != OnlineSessionState.InProgress )
            {
                Log( Util.LogLevel.Warning, "Can't leave session right now" );
                return false;
            }

            return true;
        }

        private bool CanJoinMatchmakingQueue( IUniqueUserId user_id, OnlineSessionSettings session_settings )
        {
            if( !IsUserValid( user_id ) )
            {
                Log( Util.LogLevel.Warning, "Can't join matchmaking queue: Invalid user ID" );
                return false;
            }

            if( CurrentMatchmakingInfo != null )
            {
                Log( Util.LogLevel.Warning, "Can't join matchmaking queue: Already in a queue" );
                return false;
            }

            if( !session_settings.Settings.ContainsKey( StringConstants.MATCHMAKING_TIME ) )
            {
                Log( Util.LogLevel.Warning, $"Can't join matchmaking queue: Session settings must contain {StringConstants.MATCHMAKING_TIME}" );
                return false;
            }

            try
            {
                Convert.ToUInt32( session_settings.Settings[StringConstants.MATCHMAKING_TIME].Data );
            }
            catch
            {
                Log( Util.LogLevel.Warning, $"Can't join matchmaking queue: {StringConstants.MATCHMAKING_TIME} setting must be convertible to an uint" );
                return false;
            }

            return true;
        }

        private bool CanLeaveMatchmakingQueue( IUniqueUserId user_id )
        {
            if( !IsUserValid( user_id ) )
            {
                Log( Util.LogLevel.Warning, "Can't leave matchmaking queue : Invalid user ID" );
                return false;
            }

            if( CurrentMatchmakingInfo == null )
            {
                Log( Util.LogLevel.Warning, "Can't leave matchmaking queue : Not in queue" );
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