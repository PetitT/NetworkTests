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
        public override bool IsInitialized => Core.IsInitialized();
        public override bool IsConsole => false;
        public override string ApplicationId => USAFUCore.Get().Settings.Oculus.ApplicationId;
        public override bool IsInBackground => false;

        public override void Initialize( Settings platform_settings )
        {
            Core.AsyncInitialize().OnComplete(
                ( message ) =>
                {
                    if( !message.IsError )
                    {
                        Util.Logger.Log( Util.LogLevel.Info, "Oculus Core initialized" );
                    }
                    else
                    {
                        Util.Logger.Log( Util.LogLevel.Error, "Couldn't initialize Oculus Platform" );
                    }
                }
                );
        }
    }
}