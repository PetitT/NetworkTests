using System.Collections.Generic;
using System.Threading.Tasks;
using FishingCactus.User;
using FishingCactus.UserPresence;

namespace FishingCactus.OnlineFriends
{
    public enum EInviteStatus
    {
        Unkown,
        Accepted,
        /** Friend has sent player an invite, but it has not been accepted/rejected */
        PendingInbound,
        /** Player has sent friend an invite, but it has not been accepted/rejected */
        PendingOutbound,
        Blocked
    }

    public interface IOnlineFriend : IOnlineUser
    {
        EInviteStatus InviteStatus { get; }
        OnlineUserPresence Presence { get; }
        bool IsFavorite { get; }
    }

    public delegate void OnFriendsChangedDelegate( IUniqueUserId user_id );

    public interface IOnlineFriends
    {
        event OnFriendsChangedDelegate OnFriendsChanged;

        Task<bool> ReadFriendsList( IUniqueUserId player_id, string list_name );
        List<IOnlineFriend> GetFriendsList( IUniqueUserId player_id, string list_name );
        IOnlineFriend GetFriend( IUniqueUserId player_id, IUniqueUserId friend_id, string list_name );
        bool IsFriend( IUniqueUserId player_id, IUniqueUserId friend_id, string list_name );

        /*Task< bool > DeleteFriendsList( IUniqueUserId player_id, string list_name );
        Task< bool > SendInvite( IUniqueUserId player_id, IUniqueUserId friend_id, string list_name );
        Task< bool > AcceptInvite( IUniqueUserId player_id, IUniqueUserId friend_id, string list_name );
        Task< bool > RejectInvite( IUniqueUserId player_id, IUniqueUserId friend_id, string list_name);
        Task< bool > DeleteFriend( IUniqueUserId player_id, IUniqueUserId friend_id, string list_name);
        Task< bool > QueryRecentPlayers(const FUniqueNetId& UserId, const FString& Namespace);
        Task< bool > GetRecentPlayers(const FUniqueNetId& UserId, const FString& Namespace, TArray< TSharedRef<FOnlineRecentPlayer> >& OutRecentPlayers);
        Task< bool > BlockPlayer(int controller_index, const FUniqueNetId& PlayerId);
        Task< bool > UnblockPlayer(int controller_index, const FUniqueNetId& PlayerId);
        Task< bool > QueryBlockedPlayers(const FUniqueNetId& UserId);
        Task< bool > GetBlockedPlayers(const FUniqueNetId& UserId, TArray< TSharedRef<FOnlineBlockedPlayer> >& OutBlockedPlayers);
        */
    }
}