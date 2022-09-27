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

    private string sess = "MySession";
    private string key = "Key";

    private void Start()
    {
        USAFUCore.Get().OnlineSessions.OnUpdateSessionComplete += OnlineSessions_OnUpdateSessionComplete;
    }

    private void OnlineSessions_OnUpdateSessionComplete( string session_name, bool success )
    {
        if( !success ) { return; }
        OnlineSessionSettings settings = USAFUCore.Get().OnlineSessions.GetNamedSession( session_name ).SessionSettings;
        if( settings.Settings.ContainsKey( key ) )
        {
            currentSessionValue = settings.Settings[key].Data;
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
            OnlineSessionInfo info = USAFUCore.Get().OnlineSessions.GetNamedSession( sess ).SessionInfo as OnlineSessionInfo;
            currentSessionName = sess;
            currentSessionString = info.ConnectionString;
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
                    SessionInfo = new OnlineSessionInfo
                    {
                        ConnectionString = currentSessionString
                    }
                }
            } );

        currentSessionName = USAFUCore.Get().OnlineSessions.GetNamedSession( sess ).SessionName;
    }

    private async void SetLobbyData()
    {
        bool set = await USAFUCore.Get().OnlineSessions.UpdateSession(
            sess,
            new OnlineSessionSettings
            {
                Settings = new Dictionary<string, OnlineSessionSetting>
                {
                    { key, new OnlineSessionSetting{Data = currentSessionValue } }
                }
            } );
    }


    private void OnGUI()
    {

        if( GUI.Button( new Rect( 0, 0, 100, 50 ), "Login" ) ) { USAFUCore.Get().UserSystem.Login( 0 ); }
        if( GUI.Button( new Rect( 100, 0, 100, 50 ), "Logout" ) ) { USAFUCore.Get().UserSystem.Logout( 0 ); }

        //if( GUI.Button( new Rect( 0, 0, 100, 50 ), "QuickMatch" ) ) { Join1v1Matchmaking(); }
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
        //LobbyArrangementString = GUI.TextField( new Rect( Screen.width - 200, 0, 100, 25 ), LobbyArrangementString );
        //GUI.TextField( new Rect( Screen.width - 200, 25, 100, 25 ), PlayFabManager.Instance.LobbyManager.CurrentLobbyID );
        //LobbyName = GUI.TextField( new Rect( Screen.width - 200, 50, 100, 25 ), LobbyName );

    }
}
