using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishingCactus.User;

namespace FishingCactus.OnlineAchievements
{
    [Serializable]
    public class AchievementMapping
    {
        public string AchievementId;
        public int Index;
    }

    public class Achievement
    {
        public string Id;
        public float ProgressionPercentage;
        public int CurrentProgression;
        public int TargetProgression;
        public DateTime UnlockTime;

        public virtual bool HasProgressed( int progression, int objective )
        {
            return progression > CurrentProgression;
        }

        public void Update(AchievementUpdate update)
        {
            ProgressionPercentage = update.ProgressionPercentage;
            CurrentProgression = update.CurrentProgression;
            TargetProgression = update.TargetProgression;
        }
    }

    public class AchievementDescription
    {
        public string Title;
        public string LockedDescription;
        public string UnlockedDescription;
        public bool IsHidden;
    }

    public struct AchievementUpdate
    {
        public float ProgressionPercentage => 100.0f * CurrentProgression / TargetProgression;

        public string Id;
        public int CurrentProgression;
        public int TargetProgression;
        public bool ShowProgressionIndicator;
    }

    public class AchievementUpdater
    {
        public void AddCompletedAchievement( string achievement_id, int target_progression )
        {
            updates.Add( new AchievementUpdate
            {
                Id = achievement_id,
                CurrentProgression = target_progression,
                TargetProgression = target_progression
            } );
        }

        public void AddAchievementProgression( string achievement_id, int current_progression, int target_progression, bool show_progress = false )
        {
            updates.Add( new AchievementUpdate
            {
                Id = achievement_id,
                CurrentProgression = Math.Min(current_progression, target_progression),
                TargetProgression = target_progression,
                ShowProgressionIndicator = show_progress
            } );
        }

        public IEnumerable<AchievementUpdate> Updates => updates;
        public bool HasUpdates => updates.Count > 0;
        private readonly List<AchievementUpdate> updates = new List<AchievementUpdate>();
    }

    public interface IOnlineAchievements
    {
        void Initialize( IEnumerable< AchievementMapping > achievements_mapping );
        Task<bool> InitializeForUser( IUniqueUserId user_id );
        Task<bool> WriteAchievements( IUniqueUserId user_id, AchievementUpdater writer );
        Task<bool> QueryAchievements( IUniqueUserId user_id );
        Task<bool> QueryAchievementDescriptions( IUniqueUserId user_id );
        bool GetCachedAchievement( IUniqueUserId user_id, string achievement_id, out Achievement achievement );
        bool GetCachedAchievements( IUniqueUserId user_id, out Dictionary<string, Achievement> achievement_map );
        bool GetCachedAchievementDescription( string achievement_id, out AchievementDescription description );
        Task<bool> ResetAchievements( IUniqueUserId user_id );
    }
}