using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace PlayFabIntegration
{
    public class TitleDataManager
    {
        private event Action<Dictionary<string, string>> onGetTitleDatasEvent;

        public void GetTitleDatas(Action<Dictionary<string, string>> onGetResult, string specificKey = null)
        {
            onGetTitleDatasEvent = onGetResult;
            PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnGetTitleDatas, OnFailedToGetDatas, specificKey);
        }

        private void OnGetTitleDatas(GetTitleDataResult result)
        {
            if (result == null)
            {
                Debug.Log("Title data was null");
                onGetTitleDatasEvent?.Invoke(null);
                return;
            }

            if (result.CustomData != null)
            {
                string uniqueKey = result.CustomData.ToString();
                if (result.Data.ContainsKey(uniqueKey))
                {
                    Debug.Log($"Succesfully got result at key {uniqueKey} : {result.Data[uniqueKey]}");
                    Dictionary<string, string> uniqueData = new Dictionary<string, string>
                    {
                        {uniqueKey, result.Data[uniqueKey] }
                    };
                    onGetTitleDatasEvent?.Invoke(uniqueData);
                }
                else
                {
                    Debug.Log($"Title datas do not contain {uniqueKey}");
                    onGetTitleDatasEvent?.Invoke(null);
                }
            }
            else
            {
                Debug.Log("Succesfully got title datas");
                onGetTitleDatasEvent?.Invoke(result.Data);
            }
        }

        private void OnFailedToGetDatas(PlayFabError error)
        {
            Debug.Log($"Couldn't get title data {error.GenerateErrorReport()}");
            onGetTitleDatasEvent?.Invoke(null);
        }
    }
}
