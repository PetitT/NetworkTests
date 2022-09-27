using FishingCactus;
using FishingCactus.User;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FishingCactus.Util.Logger;

public static class HelperFunctions
{
    public static bool IsControllerValid( int controller_id )
    {
        if( controller_id < 0
            || controller_id >= USAFUCore.Get().Platform.MaxLocalPlayers )
        {
            Log( FishingCactus.Util.LogLevel.Warning, "Invalid controller id" );
            return false;
        }

        return true;
    }

    public static bool IsUserValid( IUniqueUserId user_id )
    {
        if( user_id == null
            || !user_id.IsValid )
        {
            Log( FishingCactus.Util.LogLevel.Warning, "Invalid user id" );
            return false;
        }
        return true;
    }
}
