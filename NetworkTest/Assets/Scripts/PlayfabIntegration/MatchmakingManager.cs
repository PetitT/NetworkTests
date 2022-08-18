using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.MultiplayerModels;
using Mirror;
using System;

namespace PlayFabIntegration
{
    public class MatchmakingManager
    {
        private PlayFabManager playFabManager => PlayFabManager.Instance;

        private string currentQueueName;
        private string ticketID;
        private bool isPollingTicket = false;
        private float remainingTimeToPollTicket;

        private const float timeToPollTicket = 6f; //A matchmaking ticket can only be polled 10x/minutes
        private const string entityType = "title_player_account";

        public string Status { get; private set; }
        public GetMatchResult MatchResult { get; private set; }

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
            PlayFabLogging.Log("Attempt to start matchmaking");
            Status = "Status : Creating ticket";

            currentQueueName = queueName;

            var request = new CreateMatchmakingTicketRequest
            {
                GiveUpAfterSeconds = giveUpAfterSeconds,
                QueueName = currentQueueName,
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
                }
            };

            PlayFabMultiplayerAPI.CreateMatchmakingTicket(
                request,
                OnMatchmakingTicketCreated,
                (error) => PlayFabLogging.LogError("Failed to create matchmaking ticket", error)
            );
        }

        private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult result)
        {
            PlayFabLogging.Log("Succesfully created matchmaking ticket");
            Status = "Status: Ticket Created";
            ticketID = result.TicketId;
            isPollingTicket = true;
            remainingTimeToPollTicket = 0;
        }

        private void PollTicket()
        {
            PlayFabLogging.Log("Polling ticket...");

            var request = new GetMatchmakingTicketRequest
            {
                TicketId = ticketID,
                QueueName = currentQueueName
            };

            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                request,
                OnGetMatchmakingTicket,
                (error) => PlayFabLogging.LogError("Couldn't get matchmaking ticket", error)
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

        private void StartMatch(string matchID)
        {
            Status = $"Starting Match";

            var request = new GetMatchRequest
            {
                MatchId = matchID,
                QueueName = currentQueueName
            };

            PlayFabMultiplayerAPI.GetMatch(
                request,
                OnGetMatch,
                (error) => PlayFabLogging.LogError("Couldn't start match", error)
                );
        }

        private void OnGetMatch(GetMatchResult result)
        {
            Status = "Found match";
            MatchResult = result;
            PlayFabLogging.Log($"Server details : {result.ServerDetails.IPV4Address} - {result.ServerDetails.Ports[0].Num}");
            //----TODO : CLEAN THIS --- 
            CustomNetworkManager manager = NetworkManager.singleton as CustomNetworkManager;
            manager.ConnectToServer(result.ServerDetails.IPV4Address, (ushort)result.ServerDetails.Ports[0].Num);
        }

        #endregion

        #region Cancel Matchmaking

        /// <summary>
        /// Attemps to cancel the current matchmaking ticket
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!isPollingTicket)
            {
                PlayFabLogging.Log("Can't cancel matchmaking - Not in a queue");
                return;
            }

            var request = new CancelMatchmakingTicketRequest
            {
                TicketId = ticketID,
                QueueName = currentQueueName
            };

            PlayFabMultiplayerAPI.CancelMatchmakingTicket(
                request,
                OnCanceledMatchmakingTicket,
                (error) => PlayFabLogging.LogError("Couldn't cancel matchmaking ticket", error)
                );
        }

        private void OnCanceledMatchmakingTicket(CancelMatchmakingTicketResult result)
        {
            Cancel();
        }

        public void CancelAllMatchmakingQueuesForUser()
        {
            if (!isPollingTicket)
            {
                PlayFabLogging.Log("Can't cancel matchmaking - Not in a queue");
                return;
            }

            var request = new CancelAllMatchmakingTicketsForPlayerRequest
            {
                QueueName = currentQueueName
            };

            PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(
                request,
                OnCanceledAllMatchmaking,
                (error) => PlayFabLogging.LogError("Couldn't cancel all matchmaking tickets", error)
                );
        }

        private void OnCanceledAllMatchmaking(CancelAllMatchmakingTicketsForPlayerResult result)
        {
            PlayFabLogging.Log("All matcmaking tickets were canceled for this queue");
            Cancel();
        }

        private void Cancel()
        {
            isPollingTicket = false;
            Status = "Status: Canceled";
            PlayFabLogging.Log("Matchmaking was canceled");
        }

        #endregion        
    }
}
