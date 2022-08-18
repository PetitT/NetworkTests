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
        private event Action<List<PlayerLeaderboardEntry>> onGetLeaderboardEvent;

        #region Send Datas
        public void SendDataToLeaderboard(string leaderboardName, int newValue)
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
        #endregion

        #region Get Leaderboards

        /// <summary>
        /// Attemps to get a leaderboard
        /// </summary>
        /// <param name="leaderboardName"></param>
        /// <param name="maxResultsCount">Number of results, will return an empty list if set to 0</param>
        /// <param name="onGetLeaderboard">Callback returning a list of leaderboard entries</param>
        /// <param name="startPosition">Initial position of the leaderboard entries(</param>
        public void GetLeaderboard(string leaderboardName, int maxResultsCount, Action<List<PlayerLeaderboardEntry>> onGetLeaderboard, int startPosition = 0)
        {
            onGetLeaderboardEvent = onGetLeaderboard;

            var request = new GetLeaderboardRequest
            {
                StatisticName = leaderboardName,
                MaxResultsCount = maxResultsCount,
                StartPosition = startPosition
            };

            PlayFabClientAPI.GetLeaderboard(
                request, 
                OnGetLeaderboard, 
                OnFailedToGetLeaderboard);
        }

        private void OnGetLeaderboard(GetLeaderboardResult result)
        {
            PlayFabLogging.Log("Succesfully got leaderboard");
            onGetLeaderboardEvent?.Invoke(result.Leaderboard);
        }

        private void OnFailedToGetLeaderboard(PlayFabError error)
        {
            PlayFabLogging.LogError("Couldn't get leaderboard", error);
            onGetLeaderboardEvent?.Invoke(null);
        }

        /// <summary>
        /// Attemps to get a leaderboard. The entries will be centered around the player making the request
        /// </summary>
        /// <param name="Leaderboard"></param>
        /// <param name="maxResults">Number of results around the player, will return an empty list if set to 0</param>
        /// <param name="onComplete">Callback returning the list of entries</param>
        public void GetLeaderboardAroundPlayer(string Leaderboard, int maxResults, Action<List<PlayerLeaderboardEntry>> onComplete)
        {
            onGetLeaderboardEvent = onComplete;

            var request = new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = Leaderboard,
                MaxResultsCount = maxResults
            };

            PlayFabClientAPI.GetLeaderboardAroundPlayer(
                request, 
                OnGetLeaderboardAroundPlayer, 
                OnFailedToGetLeaderboardAroundPlayer
                );
        }

        private void OnGetLeaderboardAroundPlayer(GetLeaderboardAroundPlayerResult results)
        {
            PlayFabLogging.Log("Succesfully got leaderboard around player");
            onGetLeaderboardEvent?.Invoke(results.Leaderboard);
        }

        private void OnFailedToGetLeaderboardAroundPlayer(PlayFabError error)
        {
            PlayFabLogging.LogError("Failed to get leaderboard around player", error);
            onGetLeaderboardEvent?.Invoke(null);
        }

        #endregion
    }
}
