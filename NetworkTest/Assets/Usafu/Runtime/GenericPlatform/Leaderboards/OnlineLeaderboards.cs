using FishingCactus.Setup;
using FishingCactus.User;

namespace FishingCactus.OnlineLeaderboards
{
    public class LeaderboardEntry : ILeaderboardEntry
    {
        public IUniqueUserId UserID { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }
        public LeaderboardEntry() { }
    }

    public class OnlineLeaderboard : OnlineLeaderboardsBase { 
        public override void Initialize( Settings platform_settings ) {  } 
    }
}
