using FishingCactus.Setup;
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using UnityEngine;

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
        public override string ApplicationId => USAFUCore.Get().Settings.Oculus.ApplicationId;
        public override bool IsInBackground => false;

        private bool _isInitialized = false;
        
        public override void Initialize( Settings platform_settings )
        {
            Core.AsyncInitialize( ApplicationId ).OnComplete(
                ( message ) =>
                {
                    if( !message.IsError )
                    {
                        Util.Logger.Log( Util.LogLevel.Info, "Oculus Core initialized" );
#if !UNITY_EDITOR
                        CheckEntitlement(); 
#else
                        _isInitialized = true;
#endif
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Error, "Couldn't initialize Oculus Platform" );
#if !UNITY_EDITOR
                        UnityEngine.Application.Quit();
#endif
                    }
                }
                );
        }

        private void CheckEntitlement()
        {
            Entitlements.IsUserEntitledToApplication().OnComplete(
                ( message ) =>
                {
                    if( !message.IsError )
                    {
                        Util.Logger.Log( Util.LogLevel.Info, "User is entitled to the application" );
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Error, "User is not entitled to the application" );
#if !UNITY_EDITOR
                        UnityEngine.Application.Quit();
#endif
                    }
                    _isInitialized = true;
                } );
        }

        public override void Dispose()
        {
            _isInitialized = false;
        }
    }
}