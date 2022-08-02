using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PlayFabIntegration
{
    /// <summary>
    /// This class allows one to acces the user's data
    /// </summary>
    public class PlayerDataManager
    {
        private event Action<Dictionary<string, string>> onGetAllDatasEvent;
        private event Action<string> onGetSingleDataEvent;
        private event Action<object> onGetGenericDataEvent;

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
            Debug.Log($"Succesfully updated player data {obj.CustomData}");
        }

        private void OnError(PlayFabError error)
        {
            Debug.Log($"Couldn't update player data : {error.GenerateErrorReport()}");
        }

        #endregion

        #region GET DATAS
        #region ALL DATAS
        public void GetAllPlayerDatas(Action<Dictionary<string, string>> onGetPlayerDatas)
        {
            onGetAllDatasEvent = onGetPlayerDatas;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetAllDatas, OnFailedToGetAllDatas);
        }

        private void OnGetAllDatas(GetUserDataResult result)
        {
            if (result.Data == null)
            {
                Debug.Log("User Data was null");
                onGetAllDatasEvent?.Invoke(null);
                return;
            }

            Debug.Log("Successfully got player datas");
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var item in result.Data)
            {
                dict.Add(item.Key, item.Value.Value);
            }

            onGetAllDatasEvent?.Invoke(dict);
        }

        private void OnFailedToGetAllDatas(PlayFabError error)
        {
            Debug.Log($"Couldn't get all player datas : {error.GenerateErrorReport()}");
            onGetAllDatasEvent?.Invoke(null);
        }
        #endregion
        #region SINGLE DATA
        public void GetSinglePlayerData(string key, Action<string> onGetPlayerData)
        {
            onGetSingleDataEvent = onGetPlayerData;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetSingleData, OnFailedToGetSingleData, key);
        }

        private void OnGetSingleData(GetUserDataResult result)
        {
            if (result.Data == null)
            {
                Debug.Log("User Data was null");
                onGetSingleDataEvent?.Invoke(null);
                return;
            }

            if (!result.Data.ContainsKey(result.CustomData.ToString()))
            {
                Debug.Log($"User Data didn't contain {result.CustomData}");
                onGetSingleDataEvent?.Invoke(null);
                return;
            }

            Debug.Log("Succesfully got single data");
            string value = result.Data[result.CustomData.ToString()].Value;
            onGetSingleDataEvent?.Invoke(value);
        }

        private void OnFailedToGetSingleData(PlayFabError error)
        {
            Debug.Log($"Couldn't get single player data : {error.GenerateErrorReport()}");
            onGetSingleDataEvent?.Invoke(null);
        }
        #endregion
        #region GENERIC DATA

        public class GenericDataRequestInfo<T>
        {
            public string key;
            public Action<T> onComplete;

            public GenericDataRequestInfo(string key, Action<T> onComplete)
            {
                this.key = key;
                this.onComplete = onComplete;
            }
        }

        public void GetSinglePlayerData<T>(string key, Action<T> onGetDatas) /*where T : class*/
        {
            GenericDataRequestInfo<T> newInfo = new GenericDataRequestInfo<T>(key, onGetDatas);
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetGenericData<T>, OnFailedToGetGenericData, newInfo);
        }

        private void OnGetGenericData<T>(GetUserDataResult result)
        {
            GenericDataRequestInfo<T> info = result.CustomData as GenericDataRequestInfo<T>;
            string key = info.key;

            if (result == null)
            {
                Debug.Log("User data was null");
                info.onComplete?.Invoke(default(T));
                return;
            }

            if (!result.Data.ContainsKey(key))
            {
                Debug.Log($"User data didn't contain {key}");
                info.onComplete?.Invoke(default(T));
                return;
            }

            string data = result.Data[key].Value;

            try
            {
                T newObject = JsonUtility.FromJson<T>(data);
                info.onComplete?.Invoke(newObject);
                Debug.Log($"Succesfully converted {key} to {typeof(T)}");
            }
            catch
            {
                info.onComplete?.Invoke(default(T));
                Debug.Log($"Couldn't convert {key} to {typeof(T)}");
            }

        }

        private void OnFailedToGetGenericData(PlayFabError error)
        {
            Debug.Log($"Couldn't get single player generic data : {error.GenerateErrorReport()}");
            onGetGenericDataEvent?.Invoke(null);
        }

        #endregion
        #endregion
    }
}
