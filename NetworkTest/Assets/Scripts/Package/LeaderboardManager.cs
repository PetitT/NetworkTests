using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

namespace PlayFabIntegration
{
    public class LeaderboardManager
    {
        private event Action<GetLeaderboardResult> onGetLeaderboardEvent;

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

        public void GetLeaderboard(string leaderboardName, int maxResultsCount, Action<GetLeaderboardResult> onGetLeaderboard, int startPosition = 0)
        {
            onGetLeaderboard = onGetLeaderboardEvent;

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
            onGetLeaderboardEvent?.Invoke(result);
        }

        private void OnFailedToGetLeaderboard(PlayFabError error)
        {
            Debug.Log($"Couldn't get leaderboard : {error.GenerateErrorReport()}");
            onGetLeaderboardEvent?.Invoke(null);
        }

        #endregion
    }
}
