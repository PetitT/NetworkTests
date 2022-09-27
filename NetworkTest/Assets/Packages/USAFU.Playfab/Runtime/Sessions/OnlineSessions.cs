using FishingCactus.Setup;
using FishingCactus.User;
using PlayFab.MultiplayerModels;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FishingCactus.Util.Logger;

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

        private PlayFab.MultiplayerModels.EntityKey multiplayerKey;

        private NamedOnlineSession CurrentSession;

        private OnlineSessionState CurrentSessionState; 

        public void Initialize( Settings settings )
        {
            sessionMap.Clear();
            CurrentSessionState = OnlineSessionState.NoSession;
        }

        public Task<bool> CreateSession( IUniqueUserId user_id, string session_name, OnlineSessionSettings session_settings )
        {
            if( CurrentSession != null )
            {
                Log( Util.LogLevel.Warning, "Already in a lobby" );
                OnCreateSessionComplete?.Invoke( "", false );
                return Task.FromResult( false );
            }

            if( GetNamedSession( session_name )!= null )
            {
                Log( Util.LogLevel.Warning, "Session already exists" );
                OnCreateSessionComplete?.Invoke( "", false );
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
                OwnerMigrationPolicy = OwnerMigrationPolicy.Manual
            };

            PlayFabMultiplayerAPI.CreateLobby(
                request,
                ( result ) =>
                {
                    CurrentSession = new NamedOnlineSession(
                        session_name,
                        new OnlineSession
                        {
                            OwningUserId = USAFUCore.Get().UserSystem.GetUniqueUserId( 0 )
                        } );
                    OnCreateSessionComplete?.Invoke( session_name, true );
                    Log( Util.LogLevel.Info, "Created lobby" );
                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    OnCreateSessionComplete?.Invoke( "", false );
                    Log( Util.LogLevel.Error, $"Failed to create lobby : {error.GenerateErrorReport()}" );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        public Task<bool> JoinSession( IUniqueUserId user_id, string session_name, OnlineSessionSearchResult desired_session )
        {
            if( CurrentSession != null )
            {
                Log( Util.LogLevel.Warning, "Already in a lobby" );
                OnJoinSessionComplete?.Invoke( "", JoinSessionCompleteResult.AlreadyInSession );
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Joining lobby" );

            var taskCompletionSource = new TaskCompletionSource<bool>();
            var request = new JoinLobbyRequest
            {
                MemberEntity = GetMultiplayerEntityKey(),
                ConnectionString = session_name
            };

            PlayFabMultiplayerAPI.JoinLobby(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Joined lobby" );
                    currentLobbyId = result.LobbyId;
                    OnJoinSessionComplete?.Invoke( currentLobbyId, JoinSessionCompleteResult.Success );
                    taskCompletionSource.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Couldn't join lobby : {error.GenerateErrorReport()}" );
                    taskCompletionSource.TrySetResult( false );
                } );

            return taskCompletionSource.Task;
        }

        public Task<bool> DestroySession( string session_name )
        {
            if( !IsInALobby )
            {
                Log( Util.LogLevel.Error, "Not in a lobby" );
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Leaving Lobby" );

            var taskCompletionSource = new TaskCompletionSource<bool>();
            var request = new LeaveLobbyRequest()
            {
                LobbyId = currentLobbyId,
                MemberEntity = GetMultiplayerEntityKey()
            };

            PlayFabMultiplayerAPI.LeaveLobby(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Left Lobby" );
                    currentLobbyId = "";
                    OnDestroySessionComplete?.Invoke( session_name, true );
                    taskCompletionSource.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Couldn't leave lobby {error.GenerateErrorReport()}" );
                    OnDestroySessionComplete?.Invoke( session_name, false );
                    taskCompletionSource.TrySetResult( false );
                } );

            return taskCompletionSource.Task;
        }

        public Task<bool> StartSession( string session_name )
        {
            if( !sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
        }

        public Task<bool> EndSession( string session_name )
        {
            if( sessionMap.ContainsKey( session_name ) )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( true );
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

        private readonly Dictionary<string, NamedOnlineSession> sessionMap = new Dictionary<string, NamedOnlineSession>();
    }
}