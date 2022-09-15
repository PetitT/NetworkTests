using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace FishingCactus.PlayFabIntegration
{
    public class PlayerDataManager
    { 
        public void SavePlayerData(
            string dataName, 
            object newValue
            )
        {
            string convertedValue = JsonUtility.ToJson( newValue );
            SavePlayerData( dataName, convertedValue );
        }

        public void SavePlayerData(
            string dataName, 
            string newValue
            )
        {
            Dictionary<string, string> datas = new Dictionary<string, string>()
            {
                { dataName, newValue }
            };

            SavePlayerData( datas );
        }

        public void SavePlayerData( Dictionary<string, string> datas )
        {
            PlayFabLogging.Log( "Attempt to save player data" );

            var request = new UpdateUserDataRequest
            {
                Data = datas
            };

            PlayFabClientAPI.UpdateUserData(
                request, 
                ( result ) => PlayFabLogging.Log( $"Succesfully updated player data " ), 
                ( error ) => PlayFabLogging.LogError( "Couldn't update player data", error )
                );
        }

        public void GetPlayerDatas( 
            Action<Dictionary<string, string>> onGetPlayerDatas, 
            List<string> specificKeys = null
            )
        {
            PlayFabLogging.Log( "Attempt to get player datas" );

            var request = new GetUserDataRequest
            {
                Keys = specificKeys
            };

            PlayFabClientAPI.GetUserData(
               request,
               (result) =>
               {
                   PlayFabLogging.Log( "Successfully got player datas" );
                   onGetPlayerDatas?.Invoke( GetDictionnaryFromUserDataResult( result ) );
               },
               (error) =>
               {
                   PlayFabLogging.LogError( "Couldn't get all player datas", error );
                   onGetPlayerDatas?.Invoke( null );
               });
        }    

        private Dictionary<string, string> GetDictionnaryFromUserDataResult( GetUserDataResult result )
        {
            if ( result.Data == null )
            {
                PlayFabLogging.Log( "User Data was null" );
                return null;
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach ( var item in result.Data )
            {
                dict.Add( item.Key, item.Value.Value );
            }

            return dict;
        }
    }
}
