using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public interface IOnlineUserSystem
    {
        Task< bool > QueryUserInfo( IUniqueUserId user_id, List< IUniqueUserId > queried_user_ids );
        IEnumerable< IOnlineUser > GetAllUserInfo( IUniqueUserId user_id );
        IOnlineUser GetUserInfo( IUniqueUserId user_id, IUniqueUserId queried_user_id );
    }
}