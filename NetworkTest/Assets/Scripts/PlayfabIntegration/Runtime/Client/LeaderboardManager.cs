using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace FishingCactus.PlayFabIntegration
{
    public class LeaderboardManager
    {
        public void SendScoreToLeaderboard( 
            string leaderboardName, 
            int newValue 
            )
        {
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>()
                {
                    new StatisticUpdate
                    {
                        StatisticName = leaderboardName,
                        Value = newValue
                    }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(
                request,
                ( result ) => PlayFabLogging.Log( "Successfully updated leaderboard!" ),
                ( error ) => PlayFabLogging.LogError( "Couldn't update leaderboard", error )
                );
        }

        public void GetLeaderboard(
            string leaderboardName,
            int maxResultsCount,
            Action<List<PlayerLeaderboardEntry>> onGetLeaderboard, 
            int startPosition = 0
            )
        {
            var request = new GetLeaderboardRequest
            {
                StatisticName = leaderboardName,
                MaxResultsCount = maxResultsCount,
                StartPosition = startPosition
            };

            PlayFabClientAPI.GetLeaderboard(
                request,
                ( result ) =>
                {
                    PlayFabLogging.Log( "Succesfully got leaderboard" );
                    onGetLeaderboard?.Invoke( result.Leaderboard );
                },
                ( error ) =>
                {
                    PlayFabLogging.LogError( "Couldn't get leaderboard", error );
                    onGetLeaderboard?.Invoke( null );
                });
        }

        public void GetLeaderboardAroundPlayer(
            string Leaderboard, 
            int maxResults, 
            Action<List<PlayerLeaderboardEntry>> onGetLeaderboard
            )
        {
            var request = new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = Leaderboard,
                MaxResultsCount = maxResults
            };

            PlayFabClientAPI.GetLeaderboardAroundPlayer(
                request,
                ( result ) =>
                {
                    PlayFabLogging.Log( "Succesfully got leaderboard around player" );
                    onGetLeaderboard?.Invoke( result.Leaderboard );
                },
                ( error ) =>
                {
                    PlayFabLogging.LogError( "Failed to get leaderboard around player", error );
                    onGetLeaderboard?.Invoke( null );
                });
        }
    }
}
