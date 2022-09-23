using FishingCactus.Setup;

[assembly: UnityEngine.Scripting.Preserve]
[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace FishingCactus.Platform
{
    public class Platform : PlatformBase
    {
        public override int MaxLocalPlayers => 1;
        public override string PlatformName => "Generic";
        public override bool IsInitialized => _isInitialized;
        public override bool IsConsole => false;
        public override string ApplicationId => string.Empty;
        public override bool IsInBackground => false;

        public override void Initialize( Settings platform_settings )
        {
            base.Initialize( platform_settings );

            _isInitialized = true;
        }

        public override void Dispose()
        {
            _isInitialized = false;
        }

        private bool _isInitialized;
    }
}