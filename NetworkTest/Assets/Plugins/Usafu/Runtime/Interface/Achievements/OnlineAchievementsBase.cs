using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FishingCactus.User;
using FishingCactus.Util;

namespace FishingCactus.OnlineAchievements
{
    public abstract class OnlineAchievementsBase : IOnlineAchievements
    {
        public void Initialize( IEnumerable<AchievementMapping> achievements_mapping )
        {
            achievementMappingList = achievements_mapping;
        }

        public abstract Task<bool> InitializeForUser( IUniqueUserId iser_id );

        public virtual bool GetCachedAchievement( IUniqueUserId user_id, string achievement_id, out Achievement achievement )
        {
            bool result = false;

            Dictionary<string, Achievement> achievement_map;
            if ( playerAchievements.TryGetValue( user_id, out achievement_map ) )
            {
                result = achievement_map.TryGetValue( achievement_id, out achievement );
            }
            else
            {
                achievement = new Achievement();
            }

            return result;
        }

        public bool GetCachedAchievementDescription( string achievement_id, out AchievementDescription description )
        {
            return achievementDescriptions.TryGetValue( achievement_id, out description );
        }

        public bool GetCachedAchievements( IUniqueUserId user_id, out Dictionary<string, Achievement> achievement_map )
        {
            return playerAchievements.TryGetValue( user_id, out achievement_map );
        }

        public abstract Task<bool> QueryAchievementDescriptions( IUniqueUserId user_id );
        public abstract Task<bool> QueryAchievements( IUniqueUserId user_id );

        public virtual Task<bool> ResetAchievements( IUniqueUserId user_id )
        {
            Logger.Log( LogLevel.Warning, "ResetAchievements is not implemented on this platform" );
            return Task.FromResult( false );
        }

        public abstract Task<bool> WriteAchievements( IUniqueUserId user_id, AchievementUpdater writer );

        protected virtual bool GetAchievementIndexFromId( string achievement_id, out int achievement_index )
        {
            var achievement_mapping = achievementMappingList.FirstOrDefault( achievement =>
            {
                return achievement.AchievementId == achievement_id;
            } );

            if ( achievement_mapping != null )
            {
                achievement_index = achievement_mapping.Index;
                return true;
            }

            achievement_index = -1;
            return false;
        }

        protected virtual bool GetAchievementIdFromIndex( int achievement_index, out string achievement_id )
        {
            var achievement_mapping = achievementMappingList.FirstOrDefault( achievement =>
            {
                return achievement.Index == achievement_index;
            } );

            if ( achievement_mapping != null )
            {
                achievement_id = achievement_mapping.AchievementId;
                return true;
            }

            achievement_id = string.Empty;
            return false;
        }
        protected int MappedAchievementsCount { get => achievementMappingList.Count(); }

        protected readonly Dictionary<IUniqueUserId, Dictionary<string, Achievement>> playerAchievements = new Dictionary<IUniqueUserId, Dictionary<string, Achievement>>();
        protected readonly Dictionary<string, AchievementDescription> achievementDescriptions = new Dictionary<string, AchievementDescription>();
        protected IEnumerable< AchievementMapping > achievementMappingList;
    }
}
