using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabLeaderboard : MonoBehaviour
{
    public string statisticName;
    public int scoreToSend;

    [ContextMenu("Send data")]
    public void SendDataToLeaderboard()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = statisticName,
                    Value = scoreToSend
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult obj)
    {
        Debug.Log("Successfully updated leaderboard!");
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log($"Couldn't update statistic : {error.GenerateErrorReport()}");
    }

    [ContextMenu("GetLeaderboard")]
    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0,
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboard, OnCantGetLeaderboardError);
    }

    private void OnGetLeaderboard(GetLeaderboardResult result)
    {
        foreach (var item in result.Leaderboard)
        {
            Debug.Log($"Position : {item.Position} | ID : {item.DisplayName} | Value : {item.StatValue}");
        }
    }

    private void OnCantGetLeaderboardError(PlayFabError error)
    {
        Debug.Log($"Couldn't get leaderboard : {error.GenerateErrorReport()}");
    }

}