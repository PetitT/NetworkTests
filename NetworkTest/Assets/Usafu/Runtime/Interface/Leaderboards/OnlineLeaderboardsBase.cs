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

        public virtual Task<bool> ReadLeaderboards( List<IUniqueUserId> user_ids, LeaderboardReadObject read_object )
        {
            return Task.FromResult( false );
        }

        public virtual Task<bool> ReadLeaderboardsAroundRank( int rank, uint range, LeaderboardReadObject read_object )
        {
            return Task.FromResult( false );
        }

        public virtual Task<bool> ReadLeaderboardsAroundUser( IUniqueUserId user_id, uint range, LeaderboardReadObject read_object )
        {
            return Task.FromResult( false );
        }

        public virtual Task<bool> WriteOnlinePlayerRatings( string session_name, int leaderboard_id, List<OnlinePlayerScore> player_scores )
        {
            return Task.FromResult( false );
        }
    }
}
