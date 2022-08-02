using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace PlayFabIntegration
{
    /// <summary>
    /// Allows one to send datas to a leaderboard as well as get them
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

            PlayFabClientAPI.UpdatePlayerStatistics(request, OnUpdatedLeaderboard, OnFailedToUpdateLeaderboard);
        }

        private void OnUpdatedLeaderboard(UpdatePlayerStatisticsResult result)
        {
            Debug.Log("Successfully updated leaderboard!");
        }

        private void OnFailedToUpdateLeaderboard(PlayFabError error)
        {
            Debug.Log($"Couldn't update leaderboard : {error.GenerateErrorReport()}");
        }

        #endregion

        #region Get Datas

        public void GetLeaderboard(string leaderboardName, int maxResultsCount, Action<List<PlayerLeaderboardEntry>> onGetLeaderboard, int startPosition = 0)
        {
            onGetLeaderboardEvent = onGetLeaderboard;

            var request = new GetLeaderboardRequest
            {
                StatisticName = leaderboardName,
                MaxResultsCount = maxResultsCount,
                StartPosition = startPosition
            };

            PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboard, OnFailedToGetLeaderboard);
        }

        private void OnGetLeaderboard(GetLeaderboardResult result)
        {
            Debug.Log("Succesfully got leaderboard");
            onGetLeaderboardEvent?.Invoke(result.Leaderboard);
        }

        private void OnFailedToGetLeaderboard(PlayFabError error)
        {
            Debug.Log($"Couldn't get leaderboard : {error.GenerateErrorReport()}");
            onGetLeaderboardEvent?.Invoke(null);
        }

        public void GetLeaderboardAroundPlayer(string Leaderboard, int maxResults, Action<List<PlayerLeaderboardEntry>> onComplete)
        {
            onGetLeaderboardEvent = onComplete;

            var request = new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = Leaderboard,
                MaxResultsCount = maxResults
            };

            PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetLeaderboardAroundPlayer, OnFailedToGetLeaderboardAroundPlayer);
        }

        private void OnGetLeaderboardAroundPlayer(GetLeaderboardAroundPlayerResult results)
        {
            Debug.Log("Succesfully got leaderboard around player");
            onGetLeaderboardEvent?.Invoke(results.Leaderboard);
        }

        private void OnFailedToGetLeaderboardAroundPlayer(PlayFabError error)
        {
            Debug.Log("Failed to get leaderboard around player");
            onGetLeaderboardEvent?.Invoke(null);
        }

        #endregion
    }
}
