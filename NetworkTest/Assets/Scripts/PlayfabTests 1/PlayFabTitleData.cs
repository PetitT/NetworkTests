using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayFabTitleData : MonoBehaviour
{
    public string titleData;

    [ContextMenu("Get Title Data")]
    public void GetTitleData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnGetTitleData, OnError);
    }

    private void OnGetTitleData(GetTitleDataResult result)
    {
        if(result == null)
        {
            Debug.Log("Title data was null");
            return;
        }

        if (!result.Data.ContainsKey(titleData))
        {
            Debug.Log($"Title data doesn't contain key {titleData}");
            return;
        }

        Debug.Log($"Title data at key {titleData} is {result.Data[titleData]}");
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log($"Couldn't get title data {error.GenerateErrorReport()}");
    }
}
