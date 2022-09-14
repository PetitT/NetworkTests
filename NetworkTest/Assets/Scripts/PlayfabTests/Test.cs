using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using FishingCactus.PlayFabIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private PlayFabManager playFabManager => PlayFabManager.Instance;

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
    [HideInInspector] public string lobbyName;

    private void Start()
    {
        if (autoLoginOnStart)
        {
            LoginWithDeviceID();
        }
    }

    public void LoginWithDeviceID()
    {
        Login(SystemInfo.deviceUniqueIdentifier);
    }
    public void CreateNewRandomAccount()
    {
        Login(UnityEngine.Random.Range(10000, 99999).ToString());
    }

    private void LoginManager_onSuccessfulLogIn(LoginResult result)
    {
        if (string.IsNullOrEmpty(result.PlayFabId))
        {
            playFabManager.LoginManager.UpdateDisplayName(UnityEngine.Random.Range(0, 1000).ToString());
        }
    }

    private void Login(string ID)
    {
        if (playFabManager.IsLoggedIn)
        {
            Debug.Log("Already logged in");
            return;
        }

        playFabManager.LoginManager.LogIn(ID, (result) => LoginManager_onSuccessfulLogIn(result));
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
        playFabManager.PlayerDataManager.GetPlayerDatas(OnGetAllPlayerDatas);
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
        playFabManager.PlayerDataManager.GetPlayerDatas(OnGetSinglePlayerData, new List<string> { playerDataKey });
    }

    private void OnGetSinglePlayerData(Dictionary<string, string> obj)
    {
        if (obj == null)
        {
            Debug.Log("Player data didn't contain key");
            return;
        }

        Debug.Log($"Value is {obj[playerDataKey]}");
    }

    public void GetGenericData()
    {
        playFabManager.PlayerDataManager.GetPlayerDatas(OnGetGenericPlayerData, new List<string> { playerDataKey });
    }

    private void OnGetGenericPlayerData(Dictionary<string, string> datas)
    {
        if (datas == null)
        {
            Debug.Log("Object is null");
            return;
        }

        MyClass myClass = JsonUtility.FromJson<MyClass>(datas[playerDataKey]);
        Debug.Log($"{myClass.myString}");

    }

    public void SendDataToLeaderboard()
    {
        playFabManager.LeaderboardManager.SendScoreToLeaderboard(leaderboardName, score);
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
        playFabManager.LobbyManager.CreateLobby(4, LobbyManager_onLobbyCreated);
    }

    private void LobbyManager_onLobbyCreated(CreateLobbyResult obj)
    {
        lobbyArrangementString = obj.ConnectionString;
    }

    public void JoinLobby()
    {
        playFabManager.LobbyManager.JoinLobby(lobbyArrangementString);
    }

    public void FindLobbies()
    {
        playFabManager.LobbyManager.FindLobbies(OnFoundLobbies);
    }

    private void OnFoundLobbies(List<LobbySummary> obj)
    {
        Debug.Log($"Found {obj.Count} lobbies");
    }

    public void LeaveCurrentLobby()
    {
        playFabManager.LobbyManager.LeaveCurrentLobby();
    }

    public void SetLobbyName()
    {
        playFabManager.LobbyManager.SetCurrentLobbyData(new Dictionary<string, string> { { "name", lobbyName } });
    }

    public void GetLobby()
    {
        playFabManager.LobbyManager.GetCurrentLobby(OnGotLobby);
    }

    private void OnGotLobby(Lobby result)
    {
        if (result.LobbyData == null)
        {
            Debug.Log("Lobby has no lobby data");
            return;
        }
        if (!result.LobbyData.ContainsKey("name"))
        {
            Debug.Log("Lobby has no name");
            return;
        }

        lobbyName = result.LobbyData["name"];
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
            GUI.Label(new Rect(210, 15, 200, 50), playFabManager.MatchmakingManager.Status.ToString());
            GUI.Label(new Rect(110, 115, 200, 50), $"Elo : {testElo}");

            if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 50), "Create Lobby")) { CreateLobby(); }
            if (GUI.Button(new Rect(Screen.width - 100, 50, 100, 50), "Join Lobby")) { JoinLobby(); }
            if (GUI.Button(new Rect(Screen.width - 100, 100, 100, 50), "Leave lobby")) { LeaveCurrentLobby(); }
            if (GUI.Button(new Rect(Screen.width - 100, 150, 100, 50), "Find Lobbies")) { FindLobbies(); }
            if (GUI.Button(new Rect(Screen.width - 100, 200, 100, 50), "Set Lobby Name")) { SetLobbyName(); };
            lobbyArrangementString = GUI.TextField(new Rect(Screen.width - 200, 0, 100, 25), lobbyArrangementString);
            GUI.TextField(new Rect(Screen.width - 200, 25, 100, 25), PlayFabManager.Instance.LobbyManager.currentLobbyID);
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
