using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace FishingCactus.PlayFabIntegration
{
    public class TitleDataManager
    {
        public void GetTitleDatas(
            Action<Dictionary<string, string>> onGetResult, 
            List<string> specificKeys = null
            )
        {
            PlayFabLogging.Log( "Attempt to get title datas" );

            var request = new GetTitleDataRequest
            {
                Keys = specificKeys
            };
              
            PlayFabClientAPI.GetTitleData(
                request,
                ( result ) =>
                {
                    PlayFabLogging.Log( "Succesfully got title datas" );
                    onGetResult?.Invoke( result.Data );
                },
                ( error ) =>
                {
                    PlayFabLogging.LogError( "Couldn't get title datas", error );
                    onGetResult?.Invoke( null );
                });
        }
    }
}
