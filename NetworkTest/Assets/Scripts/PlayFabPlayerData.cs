using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayFabPlayerData : MonoBehaviour
{
    [Header("Unique data")]
    public string Key;
    public string Value;

    [Header("Multi datas")]
    public string multiKey;
    public string playerName;
    public string playerCountry;
    public int elo;

    #region SAVE DATA

    [ContextMenu("Save player data")]
    public void SavePlayerData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>()
            {
                { Key, Value }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnSucceed, OnError);
    }

    [ContextMenu("Save multi player data")]
    public void SaveMultiPlayerDatas()
    {
        PlayerDatas datas = new PlayerDatas(playerName, playerCountry, elo);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {multiKey, JsonUtility.ToJson(datas) }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnSucceed, OnError);
    }

    private void OnSucceed(UpdateUserDataResult obj)
    {
        Debug.Log($"Succesfully update player data {obj.CustomData}");
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log($"Couldn't update player data : {error.GenerateErrorReport()}");
    }

    #endregion

    #region GET DATA
    [ContextMenu("Get player data")]
    public void GetPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnCantGetData);
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data == null)
        {
            Debug.Log("User Data was null !");
            return;
        }

        if (!result.Data.ContainsKey(Key))
        {
            Debug.Log($"User data doesn't contain {Key}");
            return;
        }

        PlayerDatas datas = JsonUtility.FromJson<PlayerDatas>(result.Data[Key].Value);
        Debug.Log(datas.playerCountry);

        Debug.Log($"User data at {Key} is {result.Data[Key].Value}");

    }

    private void OnCantGetData(PlayFabError error)
    {
        Debug.Log($"Couldn't get player data : {error.GenerateErrorReport()}");
    }

    #endregion
}

[Serializable]
public class PlayerDatas
{
    public string playerName;
    public string playerCountry;
    public int playerElo;

    public PlayerDatas(string name, string country, int elo)
    {
        playerName = name;
        playerCountry = country;
        playerElo = elo;
    }
}
