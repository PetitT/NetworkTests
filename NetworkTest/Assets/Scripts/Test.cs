using PlayFabIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
     public PlayFabManager playFabManager;
    [HideInInspector] public string newDisplayName;
    [HideInInspector] public string specificTitleDataKey;

    [HideInInspector] public string playerDataKey;
    [HideInInspector] public string playerDataValue;

    [HideInInspector] public MyClass myClass;

    private void Awake()
    {
        playFabManager = GetComponent<PlayFabManager>();
        playFabManager.LoginManager.onSuccessfulLogIn += LoginManager_onSuccesfullLogIn;
    }



    private void LoginManager_onSuccesfullLogIn()
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

    public void GetPlayerDatas()
    {
        playFabManager.PlayerDataManager.GetPlayerData(PlayerDataManager_onGetPlayerDataEvent);
    }

    private void PlayerDataManager_onGetPlayerDataEvent(Dictionary<string, PlayFab.ClientModels.UserDataRecord> result)
    {
        foreach (var item in result)
        {
            Debug.Log($"{item.Key} : { item.Value.Value}");
        }
    }
}

[Serializable]
public class MyClass
{
    public string whatever;
    public int number;
    public float furotu;
}
