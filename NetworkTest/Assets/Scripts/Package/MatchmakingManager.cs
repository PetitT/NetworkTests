using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.MultiplayerModels;

namespace PlayFabIntegration
{
    public class MatchmakingManager
    {
        private string currentQueueName;
        private string ticketID;
        private bool isPollingTicket = false;
        private float remainingTimeToPollTicket;
        private PlayFabManager playFabManager;

        private const float timeToPollTicket = 6f; //A matchmaking ticket can only be polled 10x/minutes
        private const string entityType = "title_player_account";

        public string Status { get; private set; }

        public MatchmakingManager(PlayFabManager playFabManager)
        {
            this.playFabManager = playFabManager;
        }

        public void Tick(float deltaTime)
        {
            if (isPollingTicket)
            {
                remainingTimeToPollTicket -= deltaTime;
                if (remainingTimeToPollTicket <= 0)
                {
                    PollTicket();
                    remainingTimeToPollTicket = timeToPollTicket;
                }
            }
        }


        #region Matchmaking
        /// <summary>
        /// Attemps to put the client in a matchmaking queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="giveUpAfterSeconds">Delay after which the player automatically leaves the queue</param>
        /// <param name="customAttributes">Attributes should correspond to the Matchmaking rules in the Playfab console (e.g.: new { elo = targetElo }) </param>
        public void StartMatchmaking(string queueName, int giveUpAfterSeconds = 30, object customAttributes = null)
        {
            currentQueueName = queueName;

            PlayFabMultiplayerAPI.CreateMatchmakingTicket(
                new CreateMatchmakingTicketRequest
                {
                    Creator = new MatchmakingPlayer
                    {
                        Entity = new EntityKey
                        {
                            Id = playFabManager.EntityID,
                            Type = entityType
                        },
                        Attributes = new MatchmakingPlayerAttributes
                        {
                            DataObject = customAttributes
                        },
                    },

                    GiveUpAfterSeconds = giveUpAfterSeconds,
                    QueueName = currentQueueName
                },

                OnMatchmakingTicketCreated,
                OnFailedToCreateMatchmaking
            );
        }

        private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult result)
        {
            Debug.Log("Succesfully created matchmaking ticket");
            Status = "Status: Ticket Created";
            ticketID = result.TicketId;
            isPollingTicket = true;
            remainingTimeToPollTicket = 0;
        }

        private void PollTicket()
        {
            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                new GetMatchmakingTicketRequest
                {
                    TicketId = ticketID,
                    QueueName = currentQueueName
                },
                OnGetMatchmakingTicket,
                OnFailedToGetMatchmakingTicket
                );

            remainingTimeToPollTicket = timeToPollTicket;
        }

        private void OnGetMatchmakingTicket(GetMatchmakingTicketResult result)
        {
            Status = $"Status: {result.Status}";

            switch (result.Status)
            {
                case "Matched":
                    isPollingTicket = false;
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
            Status = $"Starting Match";

            PlayFabMultiplayerAPI.GetMatch(
                new GetMatchRequest
                {
                    MatchId = matchId,
                    QueueName = currentQueueName
                },

                OnGetMatch,
                OnFailedToGetMatch
                );
        }

        private void OnGetMatch(GetMatchResult result)
        {
            Status = $"{result.Members[0].Entity.Id} vs {result.Members[1].Entity.Id}";
        }

        #endregion

        #region Cancel Matchmaking

        /// <summary>
        /// Attemps to cancel the current matchmaking ticket
        /// </summary>
        public void CancelMatchmaking()
        {
            PlayFabMultiplayerAPI.CancelMatchmakingTicket(
                new CancelMatchmakingTicketRequest
                {
                    TicketId = ticketID,
                    QueueName = currentQueueName
                },
                OnCanceledMatchmakingTicket,
                OnFailedToCancelMatchmakingTicket
                );
        }
        private void OnCanceledMatchmakingTicket(CancelMatchmakingTicketResult result)
        {
            Cancel();
        }

        public void CancelAllMatchmakingQueuesForUser()
        {
            PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(
                new CancelAllMatchmakingTicketsForPlayerRequest
                {
                    QueueName = currentQueueName
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

        private void Cancel()
        {
            isPollingTicket = false;
            Status = "Status: Canceled";
            Debug.Log("Matchmaking was canceled");
        }

        #endregion

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
}
