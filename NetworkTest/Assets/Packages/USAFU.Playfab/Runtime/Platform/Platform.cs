using FishingCactus.Setup;
using PlayFab.Multiplayer;

[assembly: UnityEngine.Scripting.Preserve]
[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace FishingCactus.Platform
{
    public class Platform : PlatformBase
    {
        public override int MaxLocalPlayers => 1;
        public override string PlatformName => "PlayFab";
        public override bool IsInitialized => PlayFabMultiplayer.IsInitialized;
        public override bool IsConsole => false;
        public override string ApplicationId => string.Empty;
        public override bool IsInBackground => false;

        public override void Initialize( Settings platform_settings )
        {
            PlayFabMultiplayer.Initialize();
        }

        public override void Dispose()
        {
            PlayFabMultiplayer.Uninitialize();
        }
    }
}