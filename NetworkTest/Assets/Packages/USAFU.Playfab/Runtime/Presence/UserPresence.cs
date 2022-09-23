using FishingCactus.User;
using System.Threading.Tasks;

namespace FishingCactus.UserPresence
{
    public class UserPresence : IUserPresence
    {
        #pragma warning disable CS0067 // The event 'event' is never used
        public event OnPresenceReceivedDelegate OnPresenceReceived;
        #pragma warning restore CS0067 // The event 'event' is never used

        public Task<bool> SetPresence( IUniqueUserId user_id, string status )
        {
            return Task.FromResult( true );
        }

        public Task<bool> DeletePresence( IUniqueUserId user_id )
        {
            return Task.FromResult( true );
        }

        public bool GetCachedUserPresence( IUniqueUserId user_id, ref OnlineUserPresence user_presence )
        {
            throw new System.NotImplementedException();
        }
    }
}