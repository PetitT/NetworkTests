using FishingCactus.AddOns;
using FishingCactus.ExternalUI;
using FishingCactus.OnlineAchievements;
using FishingCactus.Platform;
using FishingCactus.SaveGameSystem;
using FishingCactus.User;
using FishingCactus.UserPresence;
using FishingCactus.Input;
using FishingCactus.LifeCycle;
using FishingCactus.OnlineFriends;
using FishingCactus.Sanitizer;
using FishingCactus.Setup;
using FishingCactus.OnlineSessions;
using FishingCactus.SocialPermissions;
using FishingCactus.OnlineStatistics;

namespace FishingCactus
{
    public class USAFUCore
    {
        private static USAFUCore _instance;

        public ISaveGameSystem SaveSystem { get; set; }
        public IOnlineAchievements Achievements { get; set; }
        public IOnlineStatistics Statistics { get; set; }
        public IExternalUI ExternalUI { get; set; }
        public IPlatform Platform { get; set; }
        public IPlatformUserSystem UserSystem { get; set; }
        public IOnlineUserSystem OnlineUserSystem { get; set; }
        public IUserPresence UserPresence { get; set; }
        public IAddOnSystem AddOnSystem { get; set; }
        public IInputSystem InputSystem { get; set; }
        public IPlatformInput PlatformInput { get; set; }
        public IApplicationLifeCycle ApplicationLifeCycle { get; set; }
        public IOnlineFriends Friends { get; set; }
        public ISanitizer Sanitizer { get; set; }
        public Settings Settings { get; set; }
        public IOnlineSessions OnlineSessions { get; set; }
        public ISocialPermissions SocialPermissions { get; set; }

        public static USAFUCore Get()
        { 
            return _instance ?? ( _instance = new USAFUCore() );
        }
    }
}