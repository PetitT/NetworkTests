using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFabIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public PlayFabManager playFabManager;
    [HideInInspector] public bool autoLoginOnStart;
    [HideInInspector] public string newDisplayName;
    [HideInInspector] public string specificTitleDataKey;

    [HideInInspector] public string playerDataKey;
    [HideInInspector] public string playerDataValue;

    [HideInInspector] public MyClass myClass;

    [HideInInspector] public string leaderboardName;
    [HideInInspector] public int score;
    [HideInInspector] public int maxResultsCount;
    [HideInInspector] public int startPosition;

    [HideInInspector] public int testElo;

    [HideInInspector] public string lobbyArrangementString;
    [HideInInspector] public string lobbyID;
    [HideInInspector] public string lobbyName;

    private void Awake()
    {
        playFabManager = GetComponent<PlayFabManager>();
        playFabManager.LobbyManager.onJoinedLobby += LobbyManager_onJoinedLobby;
        playFabManager.LobbyManager.onLobbyCreated += LobbyManager_onLobbyCreated;
    }

    private void Start()
    {
        if (autoLoginOnStart)
        {
            LoginWithDeviceID();
        }
    }
    public void LoginWithDeviceID()
    {
        Login(LoginManager.LoginMethod.DeviceID);
    }
    public void CreateNewRandomAccount()
    {
        Login(LoginManager.LoginMethod.Random);
        playFabManager.LoginManager.onSuccessfulLogIn += LoginManager_onSuccessfulLogIn;
    }

    private void LoginManager_onSuccessfulLogIn(LoginResult result)
    {
        playFabManager.LoginManager.UpdateDisplayName(UnityEngine.Random.Range(0, 1000).ToString());
        playFabManager.LoginManager.onSuccessfulLogIn -= LoginManager_onSuccessfulLogIn;
    }

    private void Login(LoginManager.LoginMethod method)
    {
        if (playFabManager.IsLoggedIn)
        {
            Debug.Log("Already logged in");
            return;
        }

        playFabManager.LoginManager.LogInWithID(method);
    }

    public void GetTitleDatas()
    {
        playFabManager.TitleDataManager.GetTitleDatas(OnGetTitleDatas);
    }

    public void GetSpecificTitleData()
    {
        playFabManager.TitleDataManager.GetTitleDatas(OnGetTitleDatas, new List<string>() { specificTitleDataKey });
    }

    private void OnGetTitleDatas(Dictionary<string, string> result)
    {
        if (result == null) { Debug.Log("No result..."); return; }

        Debug.Log($"Title datas : ");
        foreach (var item in result)
        {
            Debug.Log($"{item.Key} : {item.Value}");
        }
    }

    public void SavePlayerData()
    {
        playFabManager.PlayerDataManager.SavePlayerData(playerDataKey, playerDataValue);
    }

    public void SaveDataAsJSON()
    {
        playFabManager.PlayerDataManager.SavePlayerData(playerDataKey, myClass);
    }

    public void GetAllPlayerDatas()
    {
        playFabManager.PlayerDataManager.GetAllPlayerDatas(OnGetAllPlayerDatas);
    }
    private void OnGetAllPlayerDatas(Dictionary<string, string> result)
    {
        if (result == null)
        {
            Debug.Log("Player had no data");
            return;
        }

        foreach (var item in result)
        {
            Debug.Log($"{item.Key} : { item.Value}");
        }
    }

    public void GetSpecificPlayerData()
    {
        playFabManager.PlayerDataManager.GetSinglePlayerData(playerDataKey, OnGetSinglePlayerData);
    }

    private void OnGetSinglePlayerData(string obj)
    {
        if (obj == null)
        {
            Debug.Log("Player data didn't contain key");
            return;
        }

        Debug.Log($"Value is {obj}");
    }

    public void GetGenericData()
    {
        playFabManager.PlayerDataManager.GetSinglePlayerData<MyClass>(playerDataKey, OnGetGenericPlayerData);
    }

    private void OnGetGenericPlayerData(MyClass obj)
    {
        if (obj == null)
        {
            Debug.Log("Object is null");
            return;
        }

        //Debug.Log($"BRUUUUUUUUH {obj.myString}");
    }

    public void SendDataToLeaderboard()
    {
        playFabManager.LeaderboardManager.SendDataToLeaderboard(leaderboardName, score);
    }

    public void GetDataFromLeaderboard()
    {
        playFabManager.LeaderboardManager.GetLeaderboard(leaderboardName, maxResultsCount, OnGetLeaderboard, startPosition);
    }

    public void GetDataFromLeaderboardAroundPlayer()
    {
        playFabManager.LeaderboardManager.GetLeaderboardAroundPlayer(leaderboardName, maxResultsCount, OnGetLeaderboard);
    }

    private void OnGetLeaderboard(List<PlayerLeaderboardEntry> result)
    {
        if (result == null)
        {
            Debug.Log("No leaderboard");
            return;
        }

        if (result.Count == 0)
        {
            Debug.Log("No entries");
            return;
        }

        foreach (var item in result)
        {
            string isMe = item.PlayFabId == playFabManager.PlayfabID ? "--- ME ---" : "";
            Debug.Log($"{item.Position} - {item.DisplayName} : {item.StatValue}  {isMe}");
        }
    }

    private void Join1v1Matchmaking()
    {
        playFabManager.MatchmakingManager.StartMatchmaking(
                "QuickMatch",
                30,
                new
                {
                    elo = testElo,
                    latencies = new object[]
                    {
                        new
                        {
                            region = "NorthEurope",
                            latency = 100
                        }
                    }
                }
                );
    }

    private void Join2v2Matchmaking()
    {
        playFabManager.MatchmakingManager.StartMatchmaking(
                "2vs2",
                30,
                new
                {
                    elo = testElo,
                    latencies = new object[]
                    {
                        new
                        {
                            region = "NorthEurope",
                            latency = 100
                        }
                    }
                }
                );
    }

    public void CreateLobby()
    {
        playFabManager.LobbyManager.CreateLobby();
    }

    private void LobbyManager_onLobbyCreated(CreateLobbyResult obj)
    {
        lobbyID = obj.LobbyId;
        lobbyArrangementString = obj.ConnectionString;
    }

    public void JoinLobby()
    {
        playFabManager.LobbyManager.JoinLobby(lobbyArrangementString);
    }

    private void LobbyManager_onJoinedLobby(PlayFab.MultiplayerModels.JoinLobbyResult result)
    {
        lobbyID = result.LobbyId;
        GetLobby();
    }

    public void FindLobbies()
    {
        playFabManager.LobbyManager.FindLobbies();
    }

    public void DeleteAllLobbies()
    {
        playFabManager.LobbyManager.DeleteAllLobbies();
    }

    public void LeaveCurrentLobby()
    {
        playFabManager.LobbyManager.LeaveLobby(lobbyID);
    }

    public void SetLobbyName()
    {
        playFabManager.LobbyManager.SetLobbyName(lobbyID, lobbyName);
    }

    public void GetLobby()
    {
        playFabManager.LobbyManager.GetLobby(lobbyID, OnGotLobby);
    }

    private void OnGotLobby(GetLobbyResult result)
    {
        if (result.Lobby.LobbyData == null)
        {
            Debug.Log("Lobby has no lobby data");
            return;
        }
        if (!result.Lobby.LobbyData.ContainsKey("name"))
        {
            Debug.Log("Lobby has no name");
            return;
        }

        lobbyName = result.Lobby.LobbyData["name"];
        Debug.Log(lobbyName);
    }

    private void OnGUI()
    {
        if (!playFabManager.IsLoggedIn)
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Random Login")) { CreateNewRandomAccount(); }
            if (GUI.Button(new Rect(100, 0, 100, 50), "Device Login")) { LoginWithDeviceID(); }
        }
        else
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "QuickMatch")) { Join1v1Matchmaking(); }
            if (GUI.Button(new Rect(100, 0, 100, 50), "2vs2")) { Join2v2Matchmaking(); }
            if (GUI.Button(new Rect(0, 100, 100, 50), "Add elo")) { testElo++; }
            if (GUI.Button(new Rect(0, 50, 175, 50), "Cancel all matchmaking")) { playFabManager.MatchmakingManager.CancelAllMatchmakingQueuesForUser(); }

            string displayName = playFabManager.IsLoggedIn ? $"Logged in as -{playFabManager.DisplayName}-" : "Not connected";
            GUI.Label(new Rect(5, Screen.height - 50, 200, 50), displayName);
            GUI.Label(new Rect(210, 15, 200, 50), playFabManager.MatchmakingManager.Status);
            GUI.Label(new Rect(110, 115, 200, 50), $"Elo : {testElo}");

            if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 50), "Create Lobby")) { CreateLobby(); }
            if (GUI.Button(new Rect(Screen.width - 100, 50, 100, 50), "Join Lobby")) { JoinLobby(); }
            if (GUI.Button(new Rect(Screen.width - 100, 100, 100, 50), "Leave lobby")) { LeaveCurrentLobby(); }
            if (GUI.Button(new Rect(Screen.width - 100, 150, 100, 50), "Find Lobbies")) { FindLobbies(); }
            if (GUI.Button(new Rect(Screen.width - 100, 200, 100, 50), "Set Lobby Name")) { SetLobbyName(); };
            lobbyArrangementString = GUI.TextField(new Rect(Screen.width - 200, 0, 100, 25), lobbyArrangementString);
            lobbyID = GUI.TextField(new Rect(Screen.width - 200, 25, 100, 25), lobbyID);
            lobbyName = GUI.TextField(new Rect(Screen.width - 200, 50, 100, 25), lobbyName);
        }
    }
}

[Serializable]
public class MyClass
{
    public string myString;
    public int myInt;
    public float myfloat;
}
