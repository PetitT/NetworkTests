using FishingCactus.Setup;
using System;

[assembly: UnityEngine.Scripting.Preserve]
[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace FishingCactus.Platform
{
    public class Platform : PlatformBase
    {
        public override int MaxLocalPlayers => 1;
        public override string PlatformName => "Oculus";
        public override bool IsInitialized => _isInitialized;
        public override bool IsConsole => false;
        public override string ApplicationId => string.Empty;
        public override bool IsInBackground => false;

        private bool _isInitialized = false;

        public override void Initialize( Settings platform_settings )
        {
            Oculus.Platform.Core.AsyncInitialize().OnComplete(
                ( message ) =>
                {
                    if ( !message.IsError )
                    {
                        CheckEntitlement();
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Error, "Couldn't initialize Oculus Platform" );
#if !UNITY_EDITOR
                        Application.Quit();
#endif
                    }
                }
                );
        }

        private void CheckEntitlement()
        {
            Oculus.Platform.Entitlements.IsUserEntitledToApplication().OnComplete(
                ( message ) =>
                {
                    if( !message.IsError )
                    {
                        _isInitialized = true;
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Error, "User was not entitled to the application" );
#if !UNITY_EDITOR
                        Application.Quit();
#endif
                    }
                });
        }

        public override void Dispose()
        {
            _isInitialized = false;
        }
    }
}