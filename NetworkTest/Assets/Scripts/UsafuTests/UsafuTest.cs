using FishingCactus;
using FishingCactus.Unity;
using FishingCactus.User;
using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UsafuTest : MonoBehaviour
{
    private async void Start()
    {
        USAFUCore core = USAFUCore.Get();

        while( !core.Platform.IsInitialized )
        {
            await Task.Yield();
        }

        Debug.Log( "Begin Login" );
        LoginResult result = await core.UserSystem.Login( 0 );
        Debug.Log( $"result is {result.UserId}, {core.UserSystem.GetPlayerNickname( result.UserId )}" );
        await core.OnlineSessions.JoinSession( core.UserSystem.GetUniqueUserId( 0 ), Guid.NewGuid().ToString(), new FishingCactus.OnlineSessions.OnlineSessionSearchResult() );

        await Task.Delay( 5000 );
        await core.OnlineSessions.EndSession( "" );
    }
}
