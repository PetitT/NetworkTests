#if !USAFU_INPUT_USE_REWIRED

using FishingCactus.User;

namespace FishingCactus.Input
{
    public class InputSystem : InputSystemBase
    {
        public override void AssignGamepadToPlayer( int controller_id, int player_id, IUniqueUserId unique_user_id )
        {
        }

        public override void RemoveAllGamepadsFromPlayer( int player_id )
        {
        }

        public override void RemoveGamepadFromPlayer( int controller_id, int player_id )
        {
        }

        public override bool IsAnyKeyDown( out int controller_index )
        {
            if ( UnityEngine.Input.anyKeyDown )
            {
                controller_index = 0;
                return true;
            }

            controller_index = -1;
            return false;
        }

#if UNITY_XBOXONE
        public override int GetMappedControllerIdFromXboxControllerId( ulong controller_id )
        {
            return (int)controller_id;
        }
#endif

    }
}

#endif