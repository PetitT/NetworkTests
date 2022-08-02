using PlayFab.ClientModels;
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

    private void Awake()
    {
        playFabManager = GetComponent<PlayFabManager>();
        playFabManager.LoginManager.onSuccessfulLogIn += LoginManager_onSuccessfulLogIn;
    }

    private void Start()
    {
        if (autoLoginOnStart)
        {
            Login();
        }
    }
    public void Login()
    {
        if (playFabManager.IsLoggedIn)
        {
            Debug.Log("Already logged in");
            return;
        }

        playFabManager.LoginManager.LogInWithDeviceID();
    }

    private void LoginManager_onSuccessfulLogIn()
    {
        Debug.Log("LOGGED IN");
    }

    public void GetTitleDatas()
    {
        playFabManager.TitleDataManager.GetTitleDatas(OnGetTitleDatas);
    }

    public void GetSpecificTitleData()
    {
        playFabManager.TitleDataManager.GetTitleDatas(OnGetTitleDatas, specificTitleDataKey);
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
        if(result == null)
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
        if(obj == null)
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
        if(obj == null)
        {
            Debug.Log("Object is null");
            return;
        }

        Debug.Log($"BRUUUUUUUUH {obj.myString}");
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
        if(result == null)
        {
            Debug.Log("No leaderboard");
            return;
        }

        if(result.Count == 0)
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
}

[Serializable]
public class MyClass
{
    public string myString;
    public int myInt;
    public float myfloat;
}
