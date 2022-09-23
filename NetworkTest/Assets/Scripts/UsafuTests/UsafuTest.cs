using FishingCactus;
using FishingCactus.OnlineSessions;
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
    USAFUCore core;
    private /*async*/ void Start()
    {
        core = USAFUCore.Get();
        //while (!core.Platform.IsInitialized)
        //{
        //    await Task.Yield();
        //}

        //Debug.Log("Begin Login");
        //LoginResult result = await core.UserSystem.Login(0);
        //Debug.Log($"result is {result.UserId}, {core.UserSystem.GetPlayerNickname(result.UserId)}");
        //await core.OnlineSessions.JoinSession(core.UserSystem.GetUniqueUserId(0), Guid.NewGuid().ToString(), new FishingCactus.OnlineSessions.OnlineSessionSearchResult());

        //await Task.Delay(5000);
        //await core.OnlineSessions.EndSession("");
    }

    public void EntitlementCheck()
    {
        core.UserSystem.GetUserPrivilege( core.UserSystem.GetUniqueUserId( 0 ), EUserPrivileges.CanPlay ).ToString();
    }

    

    public void Login()
    {
        core.UserSystem.Login( 0 );
    }

    public void Logout()
    {
        core.UserSystem.Logout( 0 );
    }

    public void Joinsession()
    {
        core.OnlineSessions.CreateSession( core.UserSystem.GetUniqueUserId( 0 ), "Bidule", new FishingCactus.OnlineSessions.OnlineSessionSettings() );
    }

    public void LeaveSession()
    {
        core.OnlineSessions.EndSession( "Bidule" );
    }

    public void DisplayInviteUI()
    {
        core.ExternalUI.ShowInviteUI( core.UserSystem.GetUniqueUserId( 0 ), "Bidule" );
    }

    public void GetUsername()
    {
        string nickname = core.UserSystem.GetPlayerNickname( core.UserSystem.GetUniqueUserId( 0 ) );
        Debug.Log( nickname );
    }


}
