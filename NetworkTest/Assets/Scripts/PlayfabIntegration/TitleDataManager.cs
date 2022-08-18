using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace PlayFabIntegration
{
    /// <summary>
    /// This class allows one to access the datas specific to the title
    /// </summary>
    public class TitleDataManager
    {
        private event Action<Dictionary<string, string>> onGetTitleDatasEvent;

        /// <summary>
        /// The callback returns a dictionnary containing all the title datas
        /// </summary>
        /// <param name="onGetResult"> Callback returning a dictionnary with the title datas </param>
        /// <param name="keys"> If a list of keys is specified, the dictionnary will only contain those keys </param>
        public void GetTitleDatas(Action<Dictionary<string, string>> onGetResult, List<string> keys = null)
        {
            PlayFabLogging.Log("Attempt to get title datas");
            onGetTitleDatasEvent = onGetResult;

            var request = new GetTitleDataRequest
            {
                Keys = keys
            };

            PlayFabClientAPI.GetTitleData(
                request,
                OnGetTitleDatas,
                OnFailedToGetDatas
                );
        }

        private void OnGetTitleDatas(GetTitleDataResult result)
        {
            if (result == null)
            {
                PlayFabLogging.Log("Title data was null");
                onGetTitleDatasEvent?.Invoke(null);
                return;
            }


            PlayFabLogging.Log("Succesfully got title datas");
            onGetTitleDatasEvent?.Invoke(result.Data);
        }

        private void OnFailedToGetDatas(PlayFabError error)
        {
            PlayFabLogging.LogError("Couldn't get title datas", error);
            onGetTitleDatasEvent?.Invoke(null);
        }
    }
}
