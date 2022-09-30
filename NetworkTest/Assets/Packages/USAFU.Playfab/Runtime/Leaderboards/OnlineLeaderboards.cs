using FishingCactus.User;
using System.Threading.Tasks;
using PlayFab.ClientModels;
using System.Collections.Generic;
using PlayFab;
using static FishingCactus.Util.Logger;
using static FishingCactus.Util.HelperMethods;

namespace FishingCactus.OnlineLeaderboards
{
    public class OnlineLeaderboards : OnlineLeaderboardsBase
    {


        public override Task<bool> ReadLeaderboards( List<IUniqueUserId> user_ids, LeaderboardReadObject read_object )
        {
            if( user_ids.Count != 1 )
            {
                Log( Util.LogLevel.Warning, "PlayFab Can only read leaderboard for the current user" );
                read_object.ReadState = AsyncReadState.Failed;
                return Task.FromResult( false );
            }

            if( read_object.ReadState != AsyncReadState.NotStarted )
            {
                Log( Util.LogLevel.Warning, "Must Use a new read object" );
                read_object.ReadState = AsyncReadState.Failed;
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Getting leaderboard" );

            read_object.ReadState = AsyncReadState.InProgress;
            var task_completion_source = new TaskCompletionSource<bool>();

            var request = new GetLeaderboardRequest
            {
                StatisticName = read_object.LeaderboardName,
                MaxResultsCount = 100
            };

            PlayFabClientAPI.GetLeaderboard(
                request,
                ( result ) =>
                {
                    bool has_updated = TryUpdateReadObject( read_object, result.Leaderboard );
                    task_completion_source.TrySetResult( has_updated );

                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Failed to find leaderboard : {error.GenerateErrorReport()}" );
                    read_object.ReadState = AsyncReadState.Failed;
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        public override Task<bool> ReadLeaderboardsAroundUser( IUniqueUserId user_id, uint range, LeaderboardReadObject read_object )
        {
            if( !IsUserValid( user_id ) )
            {
                read_object.ReadState = AsyncReadState.Failed;
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Getting leaderboard" );

            UniqueUserId id = user_id as UniqueUserId;
            string playfab_id = id.UniqueId;

            var task_completion_source = new TaskCompletionSource<bool>();
            var request = new GetLeaderboardAroundPlayerRequest
            {
                PlayFabId = playfab_id,
                MaxResultsCount = System.Convert.ToInt32( range ),
                StatisticName = read_object.LeaderboardName
            };

            PlayFabClientAPI.GetLeaderboardAroundPlayer(
                request,
                ( result ) =>
                {
                    bool has_updated = TryUpdateReadObject( read_object, result.Leaderboard );
                    task_completion_source.TrySetResult( has_updated );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Failed to find leaderboard {error.GenerateErrorReport()}" );
                    read_object.ReadState = AsyncReadState.Failed;
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        public override Task<bool> WriteOnlinePlayerRatings( string session_name, int leaderboard_id, List<OnlinePlayerScore> player_scores )
        {
            if( player_scores.Count != 1 )
            {
                Log( Util.LogLevel.Warning, "PlayFab doesn't support updating score for other users" );
                return Task.FromResult( false );
            }

            if( !IsUserValid( player_scores[0].UserID ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Writing to leaderboard" );
            var task_completion_source = new TaskCompletionSource<bool>();

            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>()
                {
                    new StatisticUpdate
                    {
                        StatisticName = session_name,
                        Value = player_scores[0].score
                    }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Successfully updated leaderboard" );
                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Failed to update leaderboard : {error.GenerateErrorReport()}" );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        private bool TryUpdateReadObject( LeaderboardReadObject read_object, List<PlayerLeaderboardEntry> entries )
        {
            if( entries.Count == 0 )
            {
                Log( Util.LogLevel.Warning, "Leaderboard does not exist or has no entry" );
                read_object.ReadState = AsyncReadState.Failed;
                return false;
            }
            else
            {
                Log( Util.LogLevel.Info, "Found leaderboard" );
                OnlineStatsRow[] rows = new OnlineStatsRow[entries.Count];
                for( int i = 0; i < rows.Length; i++ )
                {
                    rows[i] = new OnlineStatsRow
                    {
                        NickName = entries[i].DisplayName,
                        Value = entries[i].StatValue,
                        Rank = entries[i].Position,
                        UserID = new UniqueUserId( entries[i].PlayFabId )
                    };
                }

                read_object.Rows = rows;
                read_object.ReadState = AsyncReadState.Done;
                return true;
            }
        }

        //public override Task<bool> WriteToLeaderboard( IUniqueUserId user_id, LeaderboardWriteRequest write_request )
        //{
        //    if( !IsUserValid( user_id ) )
        //    {
        //        return Task.FromResult( false );
        //    }

        //    Log( Util.LogLevel.Info, "Writing to leaderboard" );

        //    var task_completion_source = new TaskCompletionSource<bool>();

        //    var request = new UpdatePlayerStatisticsRequest
        //    {
        //        Statistics = new List<StatisticUpdate>()
        //        {
        //            new StatisticUpdate
        //            {
        //                StatisticName = write_request.LeaderboardID,
        //                Value = write_request.NewValue
        //            }
        //        }
        //    };

        //    PlayFabClientAPI.UpdatePlayerStatistics(
        //        request,
        //        ( result ) =>
        //        {
        //            Log( Util.LogLevel.Info, "Successfully updated leaderboard" );
        //            task_completion_source.TrySetResult( true );
        //        },
        //        ( error ) =>
        //        {
        //            Log( Util.LogLevel.Error, $"Failed to update leaderboard : {error.GenerateErrorReport()}" );
        //            task_completion_source.TrySetResult( false );
        //        } );

        //    return task_completion_source.Task;
        //}

        //public override Task<Leaderboard> ReadLeaderboard( LeaderboardReadRequest read_request )
        //{
        //    Log( Util.LogLevel.Info, "Getting leaderboard" );
        //    var task_completion_source = new TaskCompletionSource<Leaderboard>();
        //    var request = new GetLeaderboardRequest
        //    {
        //        StatisticName = read_request.LeaderboardID,
        //        MaxResultsCount = read_request.MaximumResults
        //    };

        //    PlayFabClientAPI.GetLeaderboard(
        //        request,
        //        ( result ) =>
        //        {
        //            if( result.Leaderboard.Count == 0 )
        //            {
        //                Log( Util.LogLevel.Warning, "Leaderboard does not exist or has no entry" );
        //                task_completion_source.TrySetResult( new Leaderboard() );
        //            }
        //            else
        //            {
        //                Log( Util.LogLevel.Info, "Found leaderboard" );
        //                task_completion_source.TrySetResult( GetLeaderboardFromResult( result.Leaderboard, read_request.LeaderboardID ) );
        //            }
        //        },
        //        ( error ) =>
        //        {
        //            Log( Util.LogLevel.Error, $"Failed to find leaderboard : {error.GenerateErrorReport()}" );
        //            task_completion_source.TrySetResult( new Leaderboard() );
        //        } );

        //    return task_completion_source.Task;
        //}

        //public override Task<Leaderboard> ReadLeaderboardAroundUser( IUniqueUserId user_id, LeaderboardReadRequest read_request )
        //{
        //    if( !IsUserValid( user_id ) )
        //    {
        //        return Task.FromResult( new Leaderboard() );
        //    }

        //    Log( Util.LogLevel.Info, "Getting leaderboard" );

        //    UniqueUserId id = user_id as UniqueUserId;
        //    string playfab_id = id.UniqueId;

        //    var task_completion_source = new TaskCompletionSource<Leaderboard>();
        //    var request = new GetLeaderboardAroundPlayerRequest
        //    {
        //        PlayFabId = playfab_id,
        //        MaxResultsCount = read_request.MaximumResults,
        //        StatisticName = read_request.LeaderboardID
        //    };

        //    PlayFabClientAPI.GetLeaderboardAroundPlayer(
        //        request,
        //        ( result ) =>
        //        {
        //            Log( Util.LogLevel.Info, "Found leaderboard" );
        //            task_completion_source.TrySetResult( GetLeaderboardFromResult( result.Leaderboard, read_request.LeaderboardID ) );
        //        },
        //        ( error ) =>
        //        {
        //            Log( Util.LogLevel.Error, $"Failed to find leaderboard {error.GenerateErrorReport()}" );
        //            task_completion_source.TrySetResult( new Leaderboard() );
        //        } );

        //    return task_completion_source.Task;
        //}

        //private Leaderboard GetLeaderboardFromResult( List<PlayerLeaderboardEntry> result, string leaderboard_name )
        //{
        //    List<ILeaderboardEntry> leaderboardEntries = new List<ILeaderboardEntry>();
        //    for( int i = 0; i < result.Count; i++ )
        //    {
        //        PlayerLeaderboardEntry entry = result[i];

        //        leaderboardEntries.Add( new LeaderboardEntry
        //        {
        //            UserName = entry.DisplayName,
        //            Score = entry.StatValue
        //        } );
        //    }

        //    Leaderboard leaderboard = new Leaderboard()
        //    {
        //        LeaderboardID = leaderboard_name,
        //        entries = leaderboardEntries
        //    };

        //    return leaderboard;
        //}
    }
}
