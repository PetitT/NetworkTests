using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace PlayFabIntegration
{
    /// <summary>
    /// Allows one to send datas to a leaderboard as well as to retrive them
    /// </summary>
    public class LeaderboardManager
    {
        public void SendScoreToLeaderboard(string leaderboardName, int newValue)
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
                (result) => PlayFabLogging.Log("Successfully updated leaderboard!"),
                (error) => PlayFabLogging.LogError("Couldn't update leaderboard", error)
                );
        }

        /// <summary>
        /// Attemps to get a leaderboard
        /// </summary>
        /// <param name="leaderboardName"></param>
        /// <param name="maxResultsCount">Number of results, will return an empty list if set to 0</param>
        /// <param name="onGetLeaderboard">Callback returning a list of leaderboard entries</param>
        /// <param name="startPosition">Initial position of the leaderboard entries(</param>
        public void GetLeaderboard(string leaderboardName, int maxResultsCount, Action<List<PlayerLeaderboardEntry>> onGetLeaderboard, int startPosition = 0)
        {
            var request = new GetLeaderboardRequest
            {
                StatisticName = leaderboardName,
                MaxResultsCount = maxResultsCount,
                StartPosition = startPosition
            };

            PlayFabClientAPI.GetLeaderboard(
                request,
                (result) =>
                {
                    PlayFabLogging.Log("Succesfully got leaderboard");
                    onGetLeaderboard?.Invoke(result.Leaderboard);
                },
                (error) =>
                {
                    PlayFabLogging.LogError("Couldn't get leaderboard", error);
                    onGetLeaderboard?.Invoke(null);
                });
        }

        /// <summary>
        /// Attemps to get a leaderboard. The entries will be centered around the player making the request
        /// </summary>
        /// <param name="Leaderboard"></param>
        /// <param name="maxResults">Number of results around the player, will return an empty list if set to 0</param>
        /// <param name="onGetLeaderboard">Callback returning the list of entries</param>
        public void GetLeaderboardAroundPlayer(string Leaderboard, int maxResults, Action<List<PlayerLeaderboardEntry>> onGetLeaderboard)
        {
            var request = new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = Leaderboard,
                MaxResultsCount = maxResults
            };

            PlayFabClientAPI.GetLeaderboardAroundPlayer(
                request,
                (result) =>
                {
                    PlayFabLogging.Log("Succesfully got leaderboard around player");
                    onGetLeaderboard?.Invoke(result.Leaderboard);
                },
                (error) =>
                {
                    PlayFabLogging.LogError("Failed to get leaderboard around player", error);
                    onGetLeaderboard?.Invoke(null);
                });
        }
    }
}
