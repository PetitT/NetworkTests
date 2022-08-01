using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayFabIntegration
{
    public class PlayerDataManager
    {
        public event Action<Dictionary<string, UserDataRecord>> onGetPlayerDataEvent;

        #region SAVE DATAS
        public void SavePlayerData(string dataName, string newValue)
        {
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>()
                {
                    { dataName, newValue }
                }
            };

            PlayFabClientAPI.UpdateUserData(request, OnSucceed, OnError);
        }

        public void SavePlayerData(string dataName, object newValue)
        {
            string convertedValue = JsonUtility.ToJson(newValue);
            SavePlayerData(dataName, convertedValue);
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

        #region GET DATAS

        public void GetPlayerData(Action<Dictionary<string, UserDataRecord>> onGetPlayerData)
        {
            onGetPlayerDataEvent = onGetPlayerData;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetData, OnFailedToGetData);
        }

        private void OnGetData(GetUserDataResult result)
        {
            if(result.Data == null)
            {
                Debug.Log("User Data was null");
                onGetPlayerDataEvent?.Invoke(null);
                return;
            }

            Debug.Log("Succesfully got player datas");
            onGetPlayerDataEvent?.Invoke(result.Data);
        }

        private void OnFailedToGetData(PlayFabError error)
        {
            Debug.Log($"Couldn't get player data : {error.GenerateErrorReport()}");
            onGetPlayerDataEvent?.Invoke(null);
        }

        #endregion
    }
}
