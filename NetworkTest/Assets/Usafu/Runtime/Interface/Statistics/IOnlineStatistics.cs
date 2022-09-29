using System.Threading.Tasks;
using FishingCactus.User;


namespace FishingCactus.OnlineStatistics
{
    public interface IOnlineStatistics
    {
        Task<bool> SetStatAsync(IUniqueUserId user_id, string stat_id, int value);
        Task<bool> SetStatAsync(IUniqueUserId user_id, string stat_id, double value);
        Task<bool> SetStatAsync(IUniqueUserId user_id, string stat_id, string value);
        Task<int> GetStatValueAsync(IUniqueUserId user_id, string stat_id);
    }
}
