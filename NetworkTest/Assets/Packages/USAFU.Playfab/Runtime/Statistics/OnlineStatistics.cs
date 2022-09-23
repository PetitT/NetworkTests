using FishingCactus.User;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishingCactus.OnlineStatistics
{
    public class OnlineStatistics : OnlineStatisticsBase
    {
        public override Task<bool> SetStatAsync( IUniqueUserId user_id, string stat_id, int value )
        {
            return SetStatAsync( user_id, stat_id, value.ToString() );
        }

        public override Task<bool> SetStatAsync( IUniqueUserId user_id, string stat_id, double value )
        {
            return SetStatAsync( user_id, stat_id, value.ToString() );
        }

        public override Task<bool> SetStatAsync( IUniqueUserId user_id, string stat_id, string value )
        {
            Util.Logger.Log( Util.LogLevel.Info, $"Updating {stat_id}" );
            var taskCompletionSource = new TaskCompletionSource<bool>();

            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { stat_id, value } }
            };

            PlayFabClientAPI.UpdateUserData(
                request,
                ( result ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Info, $"Updated {stat_id}" );
                    taskCompletionSource.TrySetResult( true );
                },
                ( error ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Error, $"Failed to update {stat_id} : {error.GenerateErrorReport()}" );
                    taskCompletionSource.TrySetResult( false );
                } );

            return taskCompletionSource.Task;
        }
    }
}
