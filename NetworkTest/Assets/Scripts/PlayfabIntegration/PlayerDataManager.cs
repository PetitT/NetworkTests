using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

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
        /// <summary>
        /// Save an entry in the player datas
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="newValue">The object must be serializable as it will be converted to JSON</param>
        public void SavePlayerData(string dataName, object newValue)
        {
            string convertedValue = JsonUtility.ToJson(newValue);
            SavePlayerData(dataName, convertedValue);
        }

        /// <summary>
        /// Save an entry in the player datas
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="newValue"></param>
        public void SavePlayerData(string dataName, string newValue)
        {
            Dictionary<string, string> datas = new Dictionary<string, string>()
            {
                { dataName, newValue }
            };

            SavePlayerData(datas);
        }

        /// <summary>
        /// Save entries in the player datas
        /// </summary>
        /// <param name="datas"></param>
        public void SavePlayerData(Dictionary<string, string> datas)
        {
            var request = new UpdateUserDataRequest
            {
                Data = datas
            };
            PlayFabClientAPI.UpdateUserData(request, OnSucceed, OnFailedToUpdatePlayerData);
        }

        private void OnSucceed(UpdateUserDataResult result)
        {
            Debug.Log($"Succesfully updated player data {result.CustomData}");
        }



        #endregion

        #region GET DATAS

        #region ALL DATAS
        /// <summary>
        /// Attemps to return a dictionnary containing all the player datas
        /// </summary>
        /// <param name="onGetPlayerDatas">Callback returning a dictionnary of the player datas</param>
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


        #endregion
        #region SINGLE DATA
        /// <summary>
        /// Attemps to return the player data at a specific key
        /// </summary>
        /// <param name="dataName">Key of the player data dictionnary</param>
        /// <param name="onGetPlayerData">Callback returning the data at the requested key</param>
        public void GetSinglePlayerData(string dataName, Action<string> onGetPlayerData)
        {
            onGetSingleDataEvent = onGetPlayerData;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetSingleData, OnFailedToGetSingleData, dataName);
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

        /// <summary>
        /// Attemps to return the player data as an object at the specific key
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize the data</typeparam>
        /// <param name="dataName">Key of the player data dictionnary</param>
        /// <param name="onGetDatas">Callback returning the object at the specified key</param>
        public void GetSinglePlayerData<T>(string dataName, Action<T> onGetDatas) 
        {
            GenericDataRequestInfo<T> newInfo = new GenericDataRequestInfo<T>(dataName, onGetDatas);
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

        #endregion

        #endregion

        #region ERRORS
        private void OnFailedToUpdatePlayerData(PlayFabError error)
        {
            Debug.Log($"Couldn't update player data : {error.GenerateErrorReport()}");
        }
        private void OnFailedToGetSingleData(PlayFabError error)
        {
            Debug.Log($"Couldn't get single player data : {error.GenerateErrorReport()}");
            onGetSingleDataEvent?.Invoke(null);
        }

        private void OnFailedToGetAllDatas(PlayFabError error)
        {
            Debug.Log($"Couldn't get all player datas : {error.GenerateErrorReport()}");
            onGetAllDatasEvent?.Invoke(null);
        }

        private void OnFailedToGetGenericData(PlayFabError error)
        {
            Debug.Log($"Couldn't get single player generic data : {error.GenerateErrorReport()}");
            onGetGenericDataEvent?.Invoke(null);
        }
        #endregion
    }
}
