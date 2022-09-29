using FishingCactus.User;
using System.Collections.Generic;

namespace FishingCactus.Input
{
    public delegate void OnControllerConnectionChangedDelegate( bool is_connected, int controller_index );

    public class GamepadAssignmentMap
    {
        public int GamePlayerId;
        public int ControllerId;
        public IUniqueUserId UniqueUserId;

        public GamepadAssignmentMap( int game_player_id, int controller_id, IUniqueUserId unique_user_id )
        {
            GamePlayerId = game_player_id;
            ControllerId = controller_id;
            UniqueUserId = unique_user_id;
        }
    }

    public interface IInputSystem
    {
        event OnControllerConnectionChangedDelegate OnControllerConnectionChanged;
        
        IEnumerable< GamepadAssignmentMap > GamepadAssignments { get; }
        void Initialize( Setup.Settings settings );
        bool IsAnyKeyDown( out int controller_index );
        void RemoveGamepadFromPlayer( int controller_id, int player_id );
        void RemoveAllGamepadsFromPlayer( int player_id );
        void AssignGamepadToPlayer( int controller_id, int player_id, IUniqueUserId unique_user_id );
#if UNITY_XBOXONE
        int GetMappedControllerIdFromXboxControllerId( ulong controller_id );
#endif
        void RemoveGamepadAssignment( int controller_id );
    }

    public abstract class InputSystemBase : IInputSystem
    {
        public event OnControllerConnectionChangedDelegate OnControllerConnectionChanged;

        public IEnumerable< GamepadAssignmentMap > GamepadAssignments => AssignedJoysticks;

        public virtual void Initialize( Setup.Settings settings )
        {
        }

        public abstract bool IsAnyKeyDown( out int controller_index );
        public abstract void RemoveGamepadFromPlayer( int controller_id, int player_id );
        public abstract void RemoveAllGamepadsFromPlayer( int player_id );
        public abstract void AssignGamepadToPlayer( int controller_id, int player_id, IUniqueUserId unique_user_id );
#if UNITY_XBOXONE
        public abstract int GetMappedControllerIdFromXboxControllerId( ulong controller_id );
#endif

        public void RemoveGamepadAssignment( int controller_id )
        {
            AssignedJoysticks.RemoveAll( assignment_map =>
            {
                return assignment_map.ControllerId == controller_id;
            } );
        }

        public void InvokeOnControllerConnectionChanged( bool is_connected, int controller_index )
        {
            OnControllerConnectionChanged?.Invoke( is_connected, controller_index );
        }

        protected List<GamepadAssignmentMap> AssignedJoysticks = new List<GamepadAssignmentMap>();
    }
}