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
        Task<bool> WriteToLeaderboard( IUniqueUserId user_id, WriteLeaderboardRequest request );
        Task<Leaderboard> ReadLeaderboard( ReadLeaderboardRequest request );
        Task<Leaderboard> ReadLeaderboardAroundUser( IUniqueUserId user_id, ReadLeaderboardRequest request );
        Task<Leaderboard> ReadLeaderboardAroundRank( uint rank, ReadLeaderboardRequest request );
    }

    public struct WriteLeaderboardRequest
    {
        public string LeaderboardID;
        public int NewValue;
    }

    public struct ReadLeaderboardRequest
    {
        public string LeaderboardID;
        public int MaximumResults;
    }

    public class Leaderboard
    {
        public string LeaderboardID;
        public List<ILeaderboardEntry> entries;
    }

    public interface ILeaderboardEntry
    {
        public IUniqueUserId UserID { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }
    }
}
