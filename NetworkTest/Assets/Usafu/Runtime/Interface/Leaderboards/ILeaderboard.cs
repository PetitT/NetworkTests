using FishingCactus.Setup;
using FishingCactus.User;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishingCactus.OnlineLeaderboards
{
    public interface IOnlineLeaderboards
    {
        void Initialize( Settings platform_settings );
        Task<bool> ReadLeaderboards( List<IUniqueUserId> user_ids, LeaderboardReadObject read_object );
        Task<bool> ReadLeaderboardsAroundRank( int rank, uint range, LeaderboardReadObject read_object );
        Task<bool> ReadLeaderboardsAroundUser( IUniqueUserId user_id, uint range, LeaderboardReadObject read_object );
        Task<bool> WriteOnlinePlayerRatings( string session_name, int leaderboard_id, List<OnlinePlayerScore> player_scores );
    }

    public struct LeaderboardWriteRequest
    {
        public DisplayFormat Format;
        public SortMethod SortMethod;
        public UpdateMethod UpdateMethod;
        public List<string> LeaderboardIDs;
        public string RatedStatistic;
    }

    public struct OnlinePlayerScore
    {
        public IUniqueUserId UserID;
        public int score;
        public int teamID;
    }

    public class LeaderboardReadObject
    {
        public string LeaderboardName;
        public string SortedColumn;
        public AsyncReadState ReadState = AsyncReadState.NotStarted;
        public ColumnMetadata[] ColumnMetadatas;
        public OnlineStatsRow[] Rows;
    }

    public struct ColumnMetadata
    {
        public string Name;
        public DataType DataType;
    }

    public struct OnlineStatsRow
    {
        public string NickName;
        public IUniqueUserId UserID;
        public int Rank;
        public object Value;
    }

    public enum DataType
    {
        Empty,
        Int32,
        Uint32,
        Int64,
        Uint64,
        Double,
        String,
        Float,
        Blob,
        Bool,
        Json
    }

    public enum DisplayFormat
    {
        Number,
        Seconds,
        Milliseconds
    }

    public enum SortMethod
    {
        None,
        Ascending,
        Descending
    }

    public enum UpdateMethod
    {
        KeepBest,
        Force
    }

    public enum AsyncReadState
    {
        NotStarted,
        InProgress,
        Done,
        Failed
    }
}
