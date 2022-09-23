using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class OnlineUserSystem : IOnlineUserSystem
    {
        public Task< bool > QueryUserInfo( IUniqueUserId user_id, List< IUniqueUserId > queried_user_ids )
        {
            return Task.FromResult( true );
        }

        public IEnumerable< IOnlineUser > GetAllUserInfo( IUniqueUserId user_id )
        {
            return Enumerable.Empty< IOnlineUser >();
        }

        public IOnlineUser GetUserInfo( IUniqueUserId user_id, IUniqueUserId queried_user_id )
        {
            return null;
        }
    }
}