using FishingCactus;
using FishingCactus.Util;
using FishingCactus.User;
using FishingCactus.OnlineSessions;
using FishingCactus.OnlineLeaderboards;
using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayFabUsafuTest : MonoBehaviour
{
    private string currentSessionName;
    private string currentSessionString;
    private string currentSessionValue;
    private string connectedName = "Not Connected";

    private string leaderboardName;
    private string leaderboardValue;

    private string sessionConstName = "MySession";
    private string sessionDataKey = "Key";

    private string playerDataKey;
    private string playerDataValue;

    private void Start()
    {
        USAFUCore.Get().OnlineSessions.OnUpdateSessionComplete += OnUpdateSession;
        USAFUCore.Get().UserSystem.OnLoginStatusChanged += OnLoginStatusChanged;
        USAFUCore.Get().OnlineSessions.OnMatchmakingComplete += MatchmakingComplete;
    }

    private void OnLoginStatusChanged( ELoginStatus old_status, ELoginStatus new_status, IUniqueUserId new_user_id )
    {
        if( new_status == ELoginStatus.NotLoggedIn )
        {
            connectedName = "Not Connected";
        }
        else if( new_status == ELoginStatus.LoggedIn )
        {
            connectedName = USAFUCore.Get().UserSystem.GetPlayerNickname( new_user_id );
            if( string.IsNullOrEmpty( connectedName ) )
            {
                connectedName = "...";
            }
        }
    }

    private void OnUpdateSession( string session_name, bool success )
    {
        if( !success ) { return; }
        if( USAFUCore.Get().OnlineSessions.GetNamedSession( session_name ) == null ) { return; }
        OnlineSessionSettings settings = USAFUCore.Get().OnlineSessions.GetNamedSession( session_name ).SessionSettings;
        if( settings.Settings.ContainsKey( sessionDataKey ) )
        {
            currentSessionValue = settings.Settings[sessionDataKey].Data;
            Debug.Log( $"Updated value to {currentSessionValue}" );
        }
        if( settings.Settings.ContainsKey( StringConstants.SERVER_ID ) )
        {
            Debug.Log( settings.Settings[StringConstants.SERVER_ID].Data );
        }
    }

    private void MatchmakingComplete( string session_name, bool success )
    {
        if( !success ) { return; }
        OnlineSessionSettings settings = USAFUCore.Get().OnlineSessions.GetNamedSession( session_name ).SessionSettings;
        if( settings.Settings.ContainsKey( StringConstants.SERVER_ID ) )
        {
            Debug.Log( settings.Settings[StringConstants.SERVER_ID].Data );
        }
    }

    private async void CreateLobby()
    {
        bool joined = await USAFUCore.Get().OnlineSessions.CreateSession(
              USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ),
              sessionConstName,
              new OnlineSessionSettings { NumPublicConnections = 4 }
              );

        if( joined )
        {
            IOnlineSessionInfo info = USAFUCore.Get().OnlineSessions.GetNamedSession( sessionConstName ).SessionInfo;
            currentSessionName = sessionConstName;
            currentSessionString = USAFUCore.Get().OnlineSessions.GetNamedSession( sessionConstName ).SessionSettings.Settings[StringConstants.CONNEXION_STRING].Data;
        }
    }

    private async void LeaveCurrentLobby()
    {
        bool left = await USAFUCore.Get().OnlineSessions.EndSession( currentSessionName );
        if( left )
        {
            currentSessionName = "";
            currentSessionString = "";
            currentSessionValue = "";
        }
    }

    private async void JoinLobby()
    {
        bool joined = await USAFUCore.Get().OnlineSessions.JoinSession(
            USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ),
            sessionConstName,
            new OnlineSessionSearchResult
            {
                Session = new OnlineSession
                {
                    SessionSettings = new OnlineSessionSettings
                    {
                        Settings = new Dictionary<string, OnlineSessionSetting>
                        {
                            { StringConstants.CONNEXION_STRING, new OnlineSessionSetting{ Data = currentSessionString} }
                        }
                    }
                }
            } );

        if( joined )
        {
            currentSessionName = USAFUCore.Get().OnlineSessions.GetNamedSession( sessionConstName ).SessionName;
        }
    }

    private async void SetLobbyData()
    {
        bool set = await USAFUCore.Get().OnlineSessions.UpdateSession(
            sessionConstName,
            new OnlineSessionSettings
            {
                Settings = new Dictionary<string, OnlineSessionSetting>
                {
                    { sessionDataKey, new OnlineSessionSetting{ Data = currentSessionValue } }
                }
            } );
    }

    private async void StartSession()
    {
        bool start = await USAFUCore.Get().OnlineSessions.StartSession( currentSessionName );
    }

    private async void Join1v1Matchmaking()
    {
        MatchmakingAttributes attributes = new MatchmakingAttributes
        {
            elo = 50,
            latencies = new Latencies[]
            {
                    new Latencies  {
                        region = "NorthEurope",
                        latency = 100
                    }
            }
        };
        string jsonAttributes = JsonUtility.ToJson( attributes );

        bool completed = await USAFUCore.Get().OnlineSessions.StartMatchmaking(
            new List<IUniqueUserId> { USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ) },
            "QuickMatch",
            new OnlineSessionSettings
            {
                Settings = new Dictionary<string, OnlineSessionSetting>()
                {
                    { StringConstants.MATCHMAKING_TIME, new OnlineSessionSetting { Data = "30" } },
                    { StringConstants.MATCHMAKING_ATTRIBUTES, new OnlineSessionSetting{Data = jsonAttributes } }
                }
            } );

        Debug.Log( $"Join 1V1 : {completed}" );
    }

    private async void JoinSimpleMatchmaking()
    {
        SimpleMatchmakingAttributes attributes = new SimpleMatchmakingAttributes() { elo = 0 };
        string json = JsonUtility.ToJson( attributes );

        bool completed = await USAFUCore.Get().OnlineSessions.StartMatchmaking(
            new List<IUniqueUserId> { USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ) },
            "Simple",
            new OnlineSessionSettings
            {
                Settings = new Dictionary<string, OnlineSessionSetting>
                {
                    {StringConstants.MATCHMAKING_ATTRIBUTES, new OnlineSessionSetting{ Data = json} },
                    {StringConstants.MATCHMAKING_TIME, new OnlineSessionSetting { Data = "30" } }
                }
            } );

        Debug.Log( $"Join simple : {completed}" );
    }

    private async void CancelMatchmaking()
    {
        bool canceled = await USAFUCore.Get().OnlineSessions.CancelMatchmaking(
            USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ),
            "" );

        Debug.Log( $"Canceled : {canceled}" );
    }

    private async void WriteToLeaderboard()
    {
        bool write = await USAFUCore.Get().OnlineLeaderboards.WriteOnlinePlayerRatings(
            leaderboardName, 0, new List<OnlinePlayerScore> { new OnlinePlayerScore { 
                score = Convert.ToInt32( leaderboardValue ),
                UserID = USAFUCore.Get().UserSystem.GetUniqueUserId(0)            
            } }
            );
    }

    private async void ReadLeaderboard()
    {
        var read_object = new LeaderboardReadObject()
        {
            LeaderboardName = leaderboardName
        };

        bool read = await USAFUCore.Get().OnlineLeaderboards.ReadLeaderboards(
            new List<IUniqueUserId> { USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ) },
            read_object
            );

        if( read )
        {
            Debug.Log( $"Found {read_object.Rows.Length} results" );
        }
    }

    private async void ReadLeaderboardAroundUser()
    {
        var read_object = new LeaderboardReadObject()
        {
            LeaderboardName = leaderboardName
        };

        bool read = await USAFUCore.Get().OnlineLeaderboards.ReadLeaderboardsAroundUser(
            USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ),
            3,
            read_object
            );

        if( read )
        {
            Debug.Log( $"Found {read_object.Rows.Length} results" );
        }
    }

    private async void SetPlayerData()
    {

    }

    private async void GetPlayerData()
    {
        bool get = USAFUCore.Get().UserSystem.GetUserAccount(
       USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ) )
    .GetUserAttribute( playerDataKey, out string result );

        if( get )
        {
            Debug.Log( result );
        }
        else
        {
            Debug.Log( "No such key" );
        }
    }

    private async void QueryPlayerData()
    {
        bool query = await USAFUCore.Get().OnlineUserSystem.QueryUserInfo(
             USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ),
             new List<IUniqueUserId> { USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ) }
            );
    }

    private void OnGUI()
    {
        GUI.BeginGroup( new Rect( 0, 0, 200, 200 ) );
        if( GUI.Button( new Rect( 0, 0, 100, 30 ), "Login" ) ) { USAFUCore.Get().UserSystem.Login( 0 ); }
        if( GUI.Button( new Rect( 0, 30, 100, 30 ), "Logout" ) ) { USAFUCore.Get().UserSystem.Logout( 0 ); }
        connectedName = GUI.TextField( new Rect( 0, 60, 100, 20 ), connectedName );
        GUI.EndGroup();

        GUI.BeginGroup( new Rect( 0, 90, 200, 200 ) );
        if( GUI.Button( new Rect( 0, 0, 100, 30 ), "QuickMatch" ) ) { Join1v1Matchmaking(); }
        if( GUI.Button( new Rect( 0, 30, 100, 30 ), "Simple" ) ) { JoinSimpleMatchmaking(); }
        if( GUI.Button( new Rect( 0, 60, 100, 30 ), "Cancel" ) ) { CancelMatchmaking(); }
        GUI.EndGroup();

        GUI.BeginGroup( new Rect( Screen.width - 300, 0, 300, 400 ) );
        if( GUI.Button( new Rect( 200, 0, 100, 30 ), "Create Lobby" ) ) { CreateLobby(); }
        if( GUI.Button( new Rect( 200, 30, 100, 30 ), "Join Lobby" ) ) { JoinLobby(); }
        if( GUI.Button( new Rect( 200, 60, 100, 30 ), "Leave lobby" ) ) { LeaveCurrentLobby(); }
        if( GUI.Button( new Rect( 200, 90, 100, 30 ), "Set L. Data" ) ) { SetLobbyData(); }
        if( GUI.Button( new Rect( 200, 120, 100, 30 ), "Start session" ) ) { StartSession(); }
        GUI.Label( new Rect( 0, 0, 100, 25 ), "Session string" );
        GUI.Label( new Rect( 0, 25, 100, 25 ), "Session name" );
        GUI.Label( new Rect( 0, 50, 100, 25 ), "Session value" );
        currentSessionString = GUI.TextField( new Rect( 100, 0, 100, 25 ), currentSessionString );
        currentSessionName = GUI.TextField( new Rect( 100, 25, 100, 25 ), currentSessionName );
        currentSessionValue = GUI.TextField( new Rect( 100, 50, 100, 25 ), currentSessionValue );
        GUI.EndGroup();

        GUI.BeginGroup( new Rect( Screen.width - 300, 160, 300, 300 ) );
        if( GUI.Button( new Rect( 200, 0, 100, 30 ), "Set P. Data" ) ) { SetPlayerData(); };
        if( GUI.Button( new Rect( 200, 30, 100, 30 ), "Get P. Data" ) ) { GetPlayerData(); };
        if( GUI.Button( new Rect( 200, 60, 100, 30 ), "Query P. Data" ) ) { QueryPlayerData(); };
        playerDataKey = GUI.TextField( new Rect( 100, 0, 100, 25 ), playerDataKey );
        playerDataValue = GUI.TextField( new Rect( 100, 25, 100, 25 ), playerDataValue );
        GUI.Label( new Rect( 0, 0, 100, 25 ), "P. data key" );
        GUI.Label( new Rect( 0, 25, 100, 25 ), "P. data value" );
        GUI.EndGroup();

        GUI.BeginGroup( new Rect( Screen.width - 300, 260, 300, 300 ) );
        if( GUI.Button( new Rect( 200, 0, 100, 30 ), "Write to lead." ) ) { WriteToLeaderboard(); };
        if( GUI.Button( new Rect( 200, 30, 100, 30 ), "Read lead." ) ) { ReadLeaderboard(); };
        if( GUI.Button( new Rect( 200, 60, 100, 30 ), "Read l. around." ) ) { ReadLeaderboardAroundUser(); };
        leaderboardName = GUI.TextField( new Rect( 100, 0, 100, 25 ), leaderboardName );
        leaderboardValue = GUI.TextField( new Rect( 100, 25, 100, 25 ), leaderboardValue );
        GUI.Label( new Rect( 0, 0, 100, 25 ), "Lead Name" );
        GUI.Label( new Rect( 0, 25, 100, 25 ), "Lead Value" );
        GUI.EndGroup();
    }

    [Serializable]
    public struct MatchmakingAttributes
    {
        public int elo;
        public Latencies[] latencies;
    }

    [Serializable]
    public struct Latencies
    {
        public string region;
        public int latency;
    }

    [Serializable]
    public struct SimpleMatchmakingAttributes
    {
        public int elo;
    }
}
