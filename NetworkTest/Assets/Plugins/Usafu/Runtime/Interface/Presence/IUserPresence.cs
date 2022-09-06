using FishingCactus.User;
using System.Threading.Tasks;

namespace FishingCactus.UserPresence
{
    public enum EOnlinePresenceState
    {
        Offline,
        Online,
        Away,
        ExtendedAway,
        DoNotDisturb,
        Chat
    }

    public class OnlineUserPresenceStatus
    {
        public string StatusStr { get; set; }
        public EOnlinePresenceState State { get; set; }
    }

    public class OnlineUserPresence
    {
        public IUniqueUserId SessionId { get; set; }
        public bool IsOnline { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsPlayingThisGame { get; set; }
        public bool IsJoinable { get; set; }
        public bool HasVoiceSupport { get; set; }
        public OnlineUserPresenceStatus Status { get; } = new OnlineUserPresenceStatus();
    }

    public delegate void OnPresenceReceivedDelegate( IUniqueUserId user_id, OnlineUserPresence online_user_presence );

    // :TODO: If needed, can add at a later time GetPresence, and allow SetPresence to accept a dictionary of key/status
    public interface IUserPresence
    {
        event OnPresenceReceivedDelegate OnPresenceReceived;

        Task<bool> SetPresence( IUniqueUserId user_id, string status );
        Task<bool> DeletePresence( IUniqueUserId user_id );
        bool GetCachedUserPresence( IUniqueUserId user_id, ref OnlineUserPresence user_presence );
    }
}