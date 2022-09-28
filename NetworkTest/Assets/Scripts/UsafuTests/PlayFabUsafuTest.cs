using FishingCactus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishingCactus.User;
using FishingCactus.OnlineSessions;

public class PlayFabUsafuTest : MonoBehaviour
{
    public string currentSessionName;
    public string currentSessionString;
    public string currentSessionValue;
    private string connectedName = "Not Connected";

    private string sess = "MySession";
    private string key = "Key";

    private void Start()
    {
        USAFUCore.Get().OnlineSessions.OnUpdateSessionComplete += OnUpdateSession;
        USAFUCore.Get().UserSystem.OnLoginStatusChanged += OnLoginStatusChanged;
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
        }
    }

    private void OnUpdateSession( string session_name, bool success )
    {
        if( !success ) { return; }
        if( USAFUCore.Get().OnlineSessions.GetNamedSession( session_name ) == null ) { return; }
        OnlineSessionSettings settings = USAFUCore.Get().OnlineSessions.GetNamedSession( session_name ).SessionSettings;
        if( settings.Settings.ContainsKey( key ) )
        {
            currentSessionValue = settings.Settings[key].Data;
            Debug.Log( $"Updated value to {currentSessionValue}" );
        }
    }

    private async void CreateLobby()
    {
        bool joined = await USAFUCore.Get().OnlineSessions.CreateSession(
              USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ),
              sess,
              new OnlineSessionSettings { NumPublicConnections = 4 }
              );

        if( joined )
        {
            IOnlineSessionInfo info = USAFUCore.Get().OnlineSessions.GetNamedSession( sess ).SessionInfo;
            currentSessionName = sess;
            currentSessionString = USAFUCore.Get().OnlineSessions.GetNamedSession( sess ).SessionSettings.Settings[StringConstants.CONNEXION_STRING].Data;
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
            sess,
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
            currentSessionName = USAFUCore.Get().OnlineSessions.GetNamedSession( sess ).SessionName;
        }
    }

    private async void SetLobbyData()
    {
        bool set = await USAFUCore.Get().OnlineSessions.UpdateSession(
            sess,
            new OnlineSessionSettings
            {
                Settings = new Dictionary<string, OnlineSessionSetting>
                {
                    { key, new OnlineSessionSetting{ Data = currentSessionValue } }
                }
            } );
    }

    private async void Join1v1Matchmaking()
    {
        bool completed = await USAFUCore.Get().OnlineSessions.StartMatchmaking(
            new List<IUniqueUserId> { USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ) },
            "Simple",
            new OnlineSessionSettings
            {
                Settings = new Dictionary<string, OnlineSessionSetting>
                {
                    { StringConstants.MATCHMAKING_TIME, new OnlineSessionSetting { Data = "30" } }
                }
            }
            );
    }


    private void OnGUI()
    {

        if( GUI.Button( new Rect( 0, 0, 100, 50 ), "Login" ) ) { USAFUCore.Get().UserSystem.Login( 0 ); }
        if( GUI.Button( new Rect( 100, 0, 100, 50 ), "Logout" ) ) { USAFUCore.Get().UserSystem.Logout( 0 ); }

        if( GUI.Button( new Rect( 0, 200, 100, 50 ), "QuickMatch" ) ) { Join1v1Matchmaking(); }
        //if( GUI.Button( new Rect( 100, 0, 100, 50 ), "2vs2" ) ) { Join2v2Matchmaking(); }
        //if( GUI.Button( new Rect( 0, 100, 100, 50 ), "Add elo" ) ) { TestElo++; }
        //if( GUI.Button( new Rect( 0, 50, 175, 50 ), "Cancel all matchmaking" ) ) { playFabManager.MatchmakingManager.CancelAllMatchmakingQueuesForUser(); }

        //string displayName = playFabManager.IsLoggedIn ? $"Logged in as -{playFabManager.DisplayName}-" : "Not connected";
        //GUI.Label( new Rect( 5, Screen.height - 50, 200, 50 ), displayName );
        //GUI.Label( new Rect( 210, 15, 200, 50 ), playFabManager.MatchmakingManager.Status.ToString() );
        //GUI.Label( new Rect( 110, 115, 200, 50 ), $"Elo : {TestElo}" );

        if( GUI.Button( new Rect( Screen.width - 100, 0, 100, 50 ), "Create Lobby" ) ) { CreateLobby(); }
        if( GUI.Button( new Rect( Screen.width - 100, 50, 100, 50 ), "Join Lobby" ) ) { JoinLobby(); }
        if( GUI.Button( new Rect( Screen.width - 100, 100, 100, 50 ), "Leave lobby" ) ) { LeaveCurrentLobby(); }
        if( GUI.Button( new Rect( Screen.width - 100, 150, 100, 50 ), "Set Data" ) ) { SetLobbyData(); }
        //if( GUI.Button( new Rect( Screen.width - 100, 200, 100, 50 ), "Set Lobby Name" ) ) { SetLobbyName(); };
        currentSessionString = GUI.TextField( new Rect( Screen.width - 200, 0, 100, 25 ), currentSessionString );
        currentSessionValue = GUI.TextField( new Rect( Screen.width - 200, 25, 100, 25 ), currentSessionValue );
        currentSessionName = GUI.TextField( new Rect( Screen.width - 200, 50, 100, 25 ), currentSessionName );
        connectedName = GUI.TextField( new Rect( 0, 50, 100, 20 ), connectedName );
        //GUI.TextField( new Rect( Screen.width - 200, 25, 100, 25 ), PlayFabManager.Instance.LobbyManager.CurrentLobbyID );

    }
}
