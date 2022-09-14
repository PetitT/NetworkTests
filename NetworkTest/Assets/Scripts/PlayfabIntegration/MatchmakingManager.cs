using PlayFab;
using PlayFab.MultiplayerModels;
using System;

namespace FishingCactus.PlayFabIntegration
{
    public class MatchmakingManager
    {
        //TYPES
        [Flags]
        public enum MatchmakingStatus
        {
            None = 0,
            CreatingTicket = 1 << 0,
            WaitingForPlayers = 1 << 1,
            WaitingForMatch = 1 << 2,
            WaitingForServer = 1 << 3,
            Matched = 1 << 4,
            Canceled = 1 << 5
        }

        //FIELDS
        public event Action<GetMatchResult> onGetMatch;

        private string currentQueueName;
        private string ticketID;
        private float remainingTimeToPollTicket;

        private const float timeToPollTicket = 6f; //A matchmaking ticket can only be polled 10x/minutes
        private const string entityType = "title_player_account";
        public GetMatchResult MatchResult { get; private set; }
        public MatchmakingStatus Status { get; private set; } = MatchmakingStatus.None;

        //PROPERTIES
        private bool isPollingTicket => (Status & (MatchmakingStatus.WaitingForPlayers | MatchmakingStatus.WaitingForMatch | MatchmakingStatus.WaitingForServer)) != 0;

        //METHODS

        public void Tick( float deltaTime )
        {
            if ( isPollingTicket )
            {
                remainingTimeToPollTicket -= deltaTime;
                if ( remainingTimeToPollTicket <= 0 )
                {
                    PollTicket();
                    remainingTimeToPollTicket = timeToPollTicket;
                }
            }
        }

        /// <summary>
        /// Attemps to put the client in a matchmaking queue
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="giveUpAfterSeconds">Delay after which the player automatically leaves the queue</param>
        /// <param name="customAttributes">Attributes should correspond to the Matchmaking rules in the Playfab console (e.g.: new { elo = targetElo }) </param>
        public void StartMatchmaking(
            string queueName, 
            int giveUpAfterSeconds = 30, 
            object customAttributes = null
            )
        {
            PlayFabLogging.Log( "Attempt to start matchmaking" );
            Status = MatchmakingStatus.CreatingTicket;
            currentQueueName = queueName;

            var request = new CreateMatchmakingTicketRequest
            {
                GiveUpAfterSeconds = giveUpAfterSeconds,
                QueueName = currentQueueName, 
                Creator = new MatchmakingPlayer
                {
                    Entity = new EntityKey
                    {
                        Id = PlayFabManager.Instance.EntityKey.Id,
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
                ( result ) => OnMatchmakingTicketCreated( result ),
                ( error ) => PlayFabLogging.LogError( "Failed to create matchmaking ticket", error )
            );
        }

        private void OnMatchmakingTicketCreated( CreateMatchmakingTicketResult result )
        {
            PlayFabLogging.Log( "Succesfully created matchmaking ticket" );
            Status = MatchmakingStatus.WaitingForPlayers;
            ticketID = result.TicketId;
            remainingTimeToPollTicket = timeToPollTicket;
        }

        private void PollTicket()
        {
            PlayFabLogging.Log( "Polling ticket..." );

            var request = new GetMatchmakingTicketRequest
            {
                TicketId = ticketID,
                QueueName = currentQueueName
            };

            PlayFabMultiplayerAPI.GetMatchmakingTicket(
                request,
                ( result ) => OnGetMatchmakingTicket( result ),
                ( error ) => PlayFabLogging.LogError( "Couldn't get matchmaking ticket", error )
                );

            remainingTimeToPollTicket = timeToPollTicket;
        }

        private void OnGetMatchmakingTicket( GetMatchmakingTicketResult result )
        {
            switch ( result.Status )
            {
                case "WaitingForPlayers":
                    Status = MatchmakingStatus.WaitingForPlayers;
                    break;

                case "WaitingForMatch":
                    Status = MatchmakingStatus.WaitingForMatch;
                    break;

                case "WaitingForServer":
                    Status = MatchmakingStatus.WaitingForServer;
                    break;

                case "Matched":
                    Status = MatchmakingStatus.Matched;
                    StartMatch( result.MatchId );
                    break;

                case "Canceled":
                    Status = MatchmakingStatus.Canceled;
                    OnMatchmakingCanceled();
                    break;

                default:
                    break;
            }
        }

        private void StartMatch( string matchID )
        {
            var request = new GetMatchRequest
            {
                MatchId = matchID,
                QueueName = currentQueueName
            };

            PlayFabMultiplayerAPI.GetMatch(
                request,
                ( result ) =>
                {
                    onGetMatch?.Invoke( result );
                    MatchResult = result;
                    PlayFabLogging.Log( "Found a match" );
                },
                ( error ) => PlayFabLogging.LogError( "Couldn't start match", error )
                );
        }

        public void CancelMatchmaking()
        {
            if ( !isPollingTicket )
            {
                PlayFabLogging.Log( "Can't cancel matchmaking - Not in a queue" );
                return;
            }

            var request = new CancelMatchmakingTicketRequest
            {
                TicketId = ticketID,
                QueueName = currentQueueName
            };

            PlayFabMultiplayerAPI.CancelMatchmakingTicket(
                request,
                ( result ) => OnMatchmakingCanceled(),
                ( error ) => PlayFabLogging.LogError( "Couldn't cancel matchmaking ticket", error )
                );
        }

        public void CancelAllMatchmakingQueuesForUser()
        {
            if ( !isPollingTicket )
            {
                PlayFabLogging.Log( "Can't cancel matchmaking - Not in a queue" );
                return;
            }

            var request = new CancelAllMatchmakingTicketsForPlayerRequest
            {
                QueueName = currentQueueName
            };

            PlayFabMultiplayerAPI.CancelAllMatchmakingTicketsForPlayer(
                request,
                ( result ) =>
                {
                    PlayFabLogging.Log( "All matcmaking tickets were canceled for this queue" );
                    OnMatchmakingCanceled();
                },
                ( error ) => PlayFabLogging.LogError( "Couldn't cancel all matchmaking tickets", error )
                );
        }

        private void OnMatchmakingCanceled()
        {
            Status = MatchmakingStatus.Canceled;
            PlayFabLogging.Log( "Matchmaking was canceled" );
        } 
    }
}
