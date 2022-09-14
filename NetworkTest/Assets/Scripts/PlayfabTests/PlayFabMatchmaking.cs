using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.MultiplayerModels;
using FishingCactus.PlayFabIntegration;
using System;

public class PlayFabMatchmaking : MonoBehaviour
{
    public PlayFabManager manager;
    public string queueName = "QuickMatch";
    public int cancelDelay = 10;

    public string status;

    private string ticketID;
    private Coroutine ticketPoll;

    [ContextMenu("Start Matchmaking")]
    public void StartMatchmaking(int targetElo)
    {
        PlayFabMultiplayerAPI.CreateMatchmakingTicket(
            new CreateMatchmakingTicketRequest
            {
                Creator = new MatchmakingPlayer
                {
                    Entity = new EntityKey
                    {
                        Id = manager.EntityKey.Id,
                        Type = "title_player_account"
                    },
                    Attributes = new MatchmakingPlayerAttributes
                    {
                        DataObject = new
                        {
                            elo = targetElo
                        },
                    },
                },

                GiveUpAfterSeconds = cancelDelay,
                QueueName = queueName
            },

            OnMatchmakingTicketCreated,
            OnFailedToCreateMatchmaking
        );
    }

    private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult result)
    {
        Debug.Log("Succesfully created matchmaking ticket");
        status = "Ticket Created";
        ticketID = result.TicketId;
        ticketPoll = StartCoroutine(PollTicket());
    }

    private IEnumerator PollTicket()
    {
        while (true)
        {
            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest
                {
                    TicketId = ticketID,
                    QueueName = queueName
                },
                OnGetMatchmakingTicket,
                OnFailedToGetMatchmakingTicket
                );

            yield return new WaitForSeconds(6);
        }
    }

    private void OnGetMatchmakingTicket(GetMatchmakingTicketResult result)
    {
        status = $"Status: {result.Status}";

        switch (result.Status)
        {
            case "Matched":
                StopCoroutine(ticketPoll);
                StartMatch(result.MatchId);
                break;

            case "Canceled":
                Cancel();
                break;

            default:
                break;
        }
    }

    private void StartMatch(string matchId)
    {
        status = $"Starting Match";

        PlayFabMultiplayerAPI.GetMatch(
            new GetMatchRequest
            {
                MatchId = matchId,
                QueueName = queueName
            },

            OnGetMatch,
            OnFailedToGetMatch
            );
    }

    private void OnGetMatch(GetMatchResult result)
    {
        status = $"{result.Members[0].Entity.Id} vs {result.Members[1].Entity.Id}";
    }

    [ContextMenu("Cancel matchmaking")]
    public void CancelMatchmaking()
    {
        PlayFabMultiplayerAPI.CancelMatchmakingTicket(
            new CancelMatchmakingTicketRequest
            {
                TicketId = ticketID,
                QueueName = queueName
            },
            OnCanceledMatchmakingTicket,
            OnFailedToCancelMatchmakingTicket
            );
    }

    private void OnCanceledMatchmakingTicket(CancelMatchmakingTicketResult result)
    {
        Cancel();
    }

    private void Cancel()
    {
        StopCoroutine(ticketPoll);
        status = "Status: Canceled";
        Debug.Log("Matchmaking was canceled");
    }

    [ContextMenu("Cancell all matchmaking tickets")]
    public void CancelAllMatchmakingQueuesForUser()
    {

        PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(
            new CancelAllMatchmakingTicketsForPlayerRequest
            {
                QueueName = queueName
            },
            OnCanceledAllMatchmaking,
            OnFailedToCancelAllMatchmaking
            );
    }

    private void OnCanceledAllMatchmaking(CancelAllMatchmakingTicketsForPlayerResult result)
    {
        Debug.Log("All matcmaking tickets were canceled for this queue");
        Cancel();
    }



    #region Errors
    private void OnFailedToCreateMatchmaking(PlayFabError error)
    {
        Debug.Log($"Couldn't create matchmaking ticket : {error.GenerateErrorReport()}");
    }

    private void OnFailedToGetMatchmakingTicket(PlayFabError error)
    {
        Debug.Log($"Couldn't get matchmaking ticket : {error.GenerateErrorReport()}");
    }

    private void OnFailedToGetMatch(PlayFabError error)
    {
        Debug.Log($"Couldn't get match : {error.GenerateErrorReport()}");
    }

    private void OnFailedToCancelMatchmakingTicket(PlayFabError error)
    {
        Debug.Log($"Couldn't cancel matchmaking ticket : {error.GenerateErrorReport()}");
    }

    private void OnFailedToCancelAllMatchmaking(PlayFabError error)
    {
        Debug.Log($"Couldn't cancel all matchmaking ticket : {error.GenerateErrorReport()}");
    }
    #endregion
}
