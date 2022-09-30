using FishingCactus.User;
using static FishingCactus.Util.Logger;

namespace FishingCactus.Util
{
    public static class HelperMethods
    {
        public static bool IsControllerValid( int controller_id )
        {
            if( controller_id < 0
                || controller_id >= USAFUCore.Get().Platform.MaxLocalPlayers )
            {
                Log( LogLevel.Warning, "Invalid controller id" );
                return false;
            }
            return true;
        }

        public static bool IsUserValid( IUniqueUserId user_id )
        {
            if( user_id == null
                || !user_id.IsValid )
            {
                Log( LogLevel.Warning, "Invalid user id" );
                return false;
            }
            return true;
        }
    }
}
