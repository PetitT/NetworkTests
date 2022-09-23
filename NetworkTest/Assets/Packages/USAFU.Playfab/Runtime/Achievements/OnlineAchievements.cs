using System.Threading.Tasks;
using FishingCactus.User;

namespace FishingCactus.OnlineAchievements
{
    public class OnlineAchievements : OnlineAchievementsBase
    {
        public override Task<bool> InitializeForUser( IUniqueUserId user_id )
        {
            return Task.FromResult( true );
        }

        public override Task<bool> QueryAchievementDescriptions( IUniqueUserId user_id )
        {
            return Task.FromResult( true );
        }

        public override Task<bool> QueryAchievements( IUniqueUserId user_id )
        {
            return Task.FromResult( true );
        }

        public override Task<bool> ResetAchievements( IUniqueUserId user_id )
        {
            return Task.FromResult( true );
        }

        public override Task<bool> WriteAchievements( IUniqueUserId user_id, AchievementUpdater updater )
        {
            return Task.FromResult( true );
        }
    }
}