using FishingCactus.Setup;
using FishingCactus.User;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishingCactus.OnlineLeaderboards
{
    public abstract class OnlineLeaderboardsBase : IOnlineLeaderboards
    {
        public virtual void Initialize( Settings platformSettings ) { }
        public virtual Task<bool> WriteToLeaderboard( IUniqueUserId user_id, WriteLeaderboardRequest request )
        {
            return Task.FromResult( true );
        }

        public virtual Task<Leaderboard> ReadLeaderboard( ReadLeaderboardRequest request )
        {
            return Task.FromResult( new Leaderboard() );
        }

        public virtual Task<Leaderboard> ReadLeaderboardAroundRank( uint rank, ReadLeaderboardRequest request )
        {
            return Task.FromResult( new Leaderboard() );
        }

        public virtual Task<Leaderboard> ReadLeaderboardAroundUser( IUniqueUserId user_id, ReadLeaderboardRequest request )
        {
            return Task.FromResult( new Leaderboard() );
        }
    }
}
