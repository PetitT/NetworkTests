using FishingCactus.Setup;
using FishingCactus.User;
using System.Threading.Tasks;
using PlayFab.ClientModels;
using System.Collections.Generic;
using PlayFab;

namespace FishingCactus.OnlineLeaderboards
{
    public class LeaderboardEntry : ILeaderboardEntry
    {
        public IUniqueUserId UserID { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }
        public LeaderboardEntry() { }
    }

    public class OnlineLeaderboards : OnlineLeaderboardsBase
    {
        public override Task<bool> WriteToLeaderboard( IUniqueUserId user_id, WriteLeaderboardRequest write_request )
        {
            if( user_id == null
                || !user_id.IsValid
                )
            {
                Util.Logger.Log( Util.LogLevel.Warning, "Invalid user id" );
                return Task.FromResult( false );
            }

            var task_completion_source = new TaskCompletionSource<bool>();

            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>()
                {
                    new StatisticUpdate
                    {
                        StatisticName = write_request.LeaderboardID,
                        Value = write_request.NewValue
                    }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(
                request,
                ( result ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Info, "Successfully updated leaderboard" );
                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Error, $"Failed to update leaderboard : {error.GenerateErrorReport()}" );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        public override Task<Leaderboard> ReadLeaderboard( ReadLeaderboardRequest read_request )
        {
            var task_completion_source = new TaskCompletionSource<Leaderboard>();
            var request = new GetLeaderboardRequest
            {
                StatisticName = read_request.LeaderboardID,
                MaxResultsCount = read_request.MaximumResults
            };

            PlayFabClientAPI.GetLeaderboard(
                request,
                ( result ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Info, "Found leaderboard" );
                    task_completion_source.TrySetResult( GetLeaderboardFromResult( result.Leaderboard, read_request.LeaderboardID ) );
                },
                ( error ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Error, "Failed to find leaderboard" );
                    task_completion_source.TrySetResult( new Leaderboard() );
                } );

            return task_completion_source.Task;
        }

        public override Task<Leaderboard> ReadLeaderboardAroundUser( IUniqueUserId user_id, ReadLeaderboardRequest read_request )
        {
            if( user_id == null
                || !user_id.IsValid
                )
            {
                Util.Logger.Log( Util.LogLevel.Warning, "User ID is invalid" );
                return Task.FromResult( new Leaderboard() );
            }

            UniqueUserId id = user_id as UniqueUserId;
            string play_fab_id = id.UniqueId;

            var task_completion_source = new TaskCompletionSource<Leaderboard>();
            var request = new GetLeaderboardAroundPlayerRequest
            {
                PlayFabId = play_fab_id,
                MaxResultsCount = read_request.MaximumResults,
                StatisticName = read_request.LeaderboardID
            };

            PlayFabClientAPI.GetLeaderboardAroundPlayer(
                request,
                ( result ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Info, "Found leaderboard" );
                    task_completion_source.TrySetResult( GetLeaderboardFromResult( result.Leaderboard, read_request.LeaderboardID ) );
                },
                ( error ) =>
                {
                    Util.Logger.Log( Util.LogLevel.Error, "Failed to find leaderboard" );
                    task_completion_source.TrySetResult( new Leaderboard() );
                } );

            return task_completion_source.Task;
        }

        private Leaderboard GetLeaderboardFromResult( List<PlayerLeaderboardEntry> result, string leaderboard_name )
        {
            List<ILeaderboardEntry> leaderboardEntries = new List<ILeaderboardEntry>();
            for( int i = 0; i < result.Count; i++ )
            {
                PlayerLeaderboardEntry entry = result[i];

                leaderboardEntries.Add( new LeaderboardEntry
                {
                    UserName = entry.DisplayName,
                    Score = entry.StatValue
                } );
            }

            Leaderboard leaderboard = new Leaderboard()
            {
                LeaderboardID = leaderboard_name,
                entries = leaderboardEntries
            };

            return leaderboard;
        }
    }
}
