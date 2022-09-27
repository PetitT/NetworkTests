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

namespace FishingCactus.OnlineSessions
{
    public class OnlineSessionInfo : IOnlineSessionInfo
    {
        public bool IsValid => true;
        public int NumPlayers = 0;
        public string SessionId { get; set; }
        public string ConnectionString { get; set; }
        public OnlineSessionInfo() { }
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

        private PlayFab.MultiplayerModels.EntityKey multiplayerKey;

        private Dictionary<string, NamedOnlineSession> sessionMap = new Dictionary<string, NamedOnlineSession>();
        private NamedOnlineSession CurrentSession;
        private OnlineSessionState CurrentSessionState;

        public void Initialize( Settings settings )
        {
            sessionMap.Clear();
            CurrentSessionState = OnlineSessionState.NoSession;
        }

        public Task<bool> CreateSession( IUniqueUserId user_id, string session_name, OnlineSessionSettings session_settings )
        {
            if( !CanJoinSession( session_name, user_id ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Creating a lobby" );
            CurrentSessionState = OnlineSessionState.Creating;
            var task_completion_source = new TaskCompletionSource<bool>();

            var request = new CreateLobbyRequest
            {
                Owner = GetMultiplayerEntityKey(),
                MaxPlayers = ( uint )session_settings.NumPublicConnections,
                Members = new List<Member> { new Member { MemberEntity = GetMultiplayerEntityKey() } },
                OwnerMigrationPolicy = OwnerMigrationPolicy.Automatic,
                UseConnections = true
            };

            PlayFabMultiplayerAPI.CreateLobby(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Created lobby" );
                    CurrentSession = AddNamedSession( session_name, result.LobbyId, result.ConnectionString );
                    CurrentSessionState = OnlineSessionState.InProgress;
                    OnCreateSessionComplete?.Invoke( session_name, true );
                    TickSessionCheck();
                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Failed to create lobby : {error.GenerateErrorReport()}" );
                    CurrentSessionState = OnlineSessionState.NoSession;
                    OnCreateSessionComplete?.Invoke( session_name, false );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        public Task<bool> JoinSession( IUniqueUserId user_id, string session_name, OnlineSessionSearchResult desired_session )
        {
            if( !CanJoinSession( session_name, user_id ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Joining lobby" );
            CurrentSessionState = OnlineSessionState.Pending;

            var info = desired_session.Session.SessionInfo as OnlineSessionInfo;
            var task_completion_source = new TaskCompletionSource<bool>();
            var request = new JoinLobbyRequest
            {
                MemberEntity = GetMultiplayerEntityKey(),
                ConnectionString = info.ConnectionString
            };

            PlayFabMultiplayerAPI.JoinLobby(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Joined lobby" );
                    CurrentSessionState = OnlineSessionState.InProgress;
                    CurrentSession = AddNamedSession( session_name, result.LobbyId );
                    OnJoinSessionComplete?.Invoke( session_name, JoinSessionCompleteResult.Success );
                    TickSessionCheck();
                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Couldn't join lobby : {error.GenerateErrorReport()}" );
                    CurrentSessionState = OnlineSessionState.NoSession;
                    OnJoinSessionComplete?.Invoke( session_name, JoinSessionCompleteResult.UnknownError );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        public Task<bool> EndSession( string session_name )
        {
            if( !CanLeaveSession( session_name ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Leaving Lobby" );
            CurrentSessionState = OnlineSessionState.Ending;

            var task_completion_source = new TaskCompletionSource<bool>();
            var onlineSessionInfo = GetNamedSession( session_name ).SessionInfo as OnlineSessionInfo;

            var request = new LeaveLobbyRequest()
            {
                LobbyId = onlineSessionInfo.SessionId,
                MemberEntity = GetMultiplayerEntityKey()
            };

            PlayFabMultiplayerAPI.LeaveLobby(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Left Lobby" );
                    RemoveNamedSession( session_name );
                    CurrentSession = null;
                    CurrentSessionState = OnlineSessionState.NoSession;
                    OnEndSessionComplete?.Invoke( session_name, true );
                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Couldn't leave lobby {error.GenerateErrorReport()}" );
                    CurrentSessionState = OnlineSessionState.InProgress;
                    OnEndSessionComplete?.Invoke( session_name, false );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        private async void TickSessionCheck()
        {
            // NOTE: we need to check for the session data every few seconds
            // TODO: Find a way to listen to lobby updates

            await Task.Delay( Convert.ToInt32( TimeBetweenSessionsCheck * 1000 ) );
            if( CurrentSession != null && UnityEngine.Application.isPlaying )
            {
                Log( Util.LogLevel.Info, "Checking session" );
                await GetSessionData( CurrentSession.SessionName );
                TickSessionCheck();
            }
        }

        public Task<bool> UpdateSession( string session_name, OnlineSessionSettings updated_session_settings )
        {
            if( GetNamedSession( session_name ) == null )
            {
                Log( Util.LogLevel.Warning, "Can't update session : session does not exist" );
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Updating lobby data" );
            var task_completion_source = new TaskCompletionSource<bool>();
            var session_info = GetNamedSession( session_name ).SessionInfo as OnlineSessionInfo;

            Dictionary<string, string> datas = new Dictionary<string, string>();
            foreach( var item in updated_session_settings.Settings )
            {
                datas.Add( item.Key, item.Value.Data );
            }

            var request = new UpdateLobbyRequest
            {
                MemberEntity = GetMultiplayerEntityKey(),
                LobbyId = session_info.SessionId,
                LobbyData = datas
            };

            PlayFabMultiplayerAPI.UpdateLobby(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Updated lobby" );
                    OnUpdateSessionComplete?.Invoke( session_name, true );
                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, "Failed to update lobby" );
                    OnUpdateSessionComplete?.Invoke( session_name, false );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        private Task<bool> GetSessionData( string session_name )
        {
            if( GetNamedSession( session_name ) == null )
            {
                Log( Util.LogLevel.Warning, "Can't get session data : session does not exist" );
                return Task.FromResult( false );
            }

            var task_completion_source = new TaskCompletionSource<bool>();
            var info = GetNamedSession( session_name ).SessionInfo as OnlineSessionInfo;
            var request = new GetLobbyRequest
            {
                LobbyId = info.SessionId
            };

            Log( Util.LogLevel.Info, "Getting session data" );
            PlayFabMultiplayerAPI.GetLobby(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Found lobby" );
                    var selected_session = GetNamedSession( session_name );
                    var session_info = selected_session.SessionInfo as OnlineSessionInfo;
                    session_info.NumPlayers = result.Lobby.Members.Count;

                    if( result.Lobby.LobbyData != null )
                    {
                        foreach( var item in result.Lobby.LobbyData )
                        {
                            selected_session.SessionSettings.Settings[item.Key] = new OnlineSessionSetting { Data = item.Value };
                        }
                    }

                    OnUpdateSessionComplete?.Invoke( session_name, true );
                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Couldn't find lobby {error.GenerateErrorReport()}" );
                    OnUpdateSessionComplete?.Invoke( session_name, false );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        public Task<bool> StartSession( string session_name )
        {
            if( !sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }

        public Task<bool> DestroySession( string session_name )
        {
            if( sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }

        private NamedOnlineSession AddNamedSession( string session_name, string session_id, string connection_string = "" )
        {

            NamedOnlineSession newSession = new NamedOnlineSession(
                 session_name,
                 new OnlineSession
                 {
                     SessionInfo = new OnlineSessionInfo
                     {
                         SessionId = session_id,
                         ConnectionString = connection_string
                     }
                 } );

            sessionMap.Add( session_name, newSession );
            return newSession;
        }

        private void RemoveNamedSession( string session_name )
        {
            sessionMap.Remove( session_name );
        }

        public NamedOnlineSession GetNamedSession( string session_name )
        {
            if( sessionMap.TryGetValue( session_name, out NamedOnlineSession named_online_session ) )
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

            if( CurrentSessionState != OnlineSessionState.InProgress )
            {
                Log( Util.LogLevel.Warning, "Can't leave session right now" );
                return false;
            }

            return true;
        }

        private EntityKey GetMultiplayerEntityKey()
        {
            if( multiplayerKey == null )
            {
                USAFUCore core = USAFUCore.Get();
                IUniqueUserId userID = core.UserSystem.GetUniqueUserId( 0 );
                IUserOnlineAccount account = core.UserSystem.GetUserAccount( userID );
                account.GetAuthAttributeByName( out string ID, StringConstants.ENTITY_ID );
                account.GetAuthAttributeByName( out string Type, StringConstants.ENTITY_TYPE );
                multiplayerKey = new EntityKey { Id = ID, Type = Type };
            }

            return multiplayerKey;
        }
    }
}