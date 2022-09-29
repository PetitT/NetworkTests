using FishingCactus.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingCactus.OnlineFriends
{
    public class OnlineFriends : IOnlineFriends
    {
        #pragma warning disable CS0067 // The event 'event' is never used
        public event OnFriendsChangedDelegate OnFriendsChanged;
        #pragma warning restore CS0067 // The event 'event' is never used

        public Task<bool> ReadFriendsList( IUniqueUserId user_id, string list_name )
        {
            return Task.FromResult( false );
        }

        public IOnlineFriend GetFriend( IUniqueUserId user_id, IUniqueUserId friend_id, string list_name )
        {
            return null;
        }

        public List<IOnlineFriend> GetFriendsList( IUniqueUserId user_id, string list_name )
        {
            return new List< IOnlineFriend >();
        }

        public bool IsFriend( IUniqueUserId user_id, IUniqueUserId friend_id, string list_name )
        {
            return false;
        }

        public Task<bool> IsUserMuted(IUniqueUserId user_id, string other_user_id)
        {
            return Task.FromResult( false );
        }

        /*public Task<bool> AcceptInvite( IUniqueUserId user_id, IUniqueUserId friend_id, string list_name )
        {
            return Task.FromResult( false );
        }

        public Task<bool> DeleteFriend( IUniqueUserId user_id, IUniqueUserId friend_id, string list_name )
        {
            return Task.FromResult( false );
        }

        public Task<bool> DeleteFriendsList( IUniqueUserId user_id, string list_name )
        {
            return Task.FromResult( false );
        }



        public Task<bool> RejectInvite( IUniqueUserId user_id, IUniqueUserId friend_id, string list_name )
        {
            return Task.FromResult( false );
        }

        public Task<bool> SendInvite( IUniqueUserId user_id, IUniqueUserId friend_id, string list_name )
        {
            return Task.FromResult( false );
        }*/
    }
}