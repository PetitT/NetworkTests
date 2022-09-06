using FishingCactus.User;
using System.Threading.Tasks;

namespace FishingCactus.OnlineStatistics
{
    public abstract class OnlineStatisticsBase : IOnlineStatistics
    {
        public virtual Task<bool> SetStatAsync(IUniqueUserId user_id, string stat_id, int value)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> SetStatAsync(IUniqueUserId user_id, string stat_id, double value)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> SetStatAsync(IUniqueUserId user_id, string stat_id, string value)
        {
            return Task.FromResult(true);
        }

        public virtual Task<int> GetStatValueAsync(IUniqueUserId user_id, string stat_id)
        {
            return Task.FromResult(0);
        }
    }
}
