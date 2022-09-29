#if USAFU_INPUT_USE_REWIRED

using FishingCactus.Setup;
using FishingCactus.User;
using FishingCactus.Util;
using Rewired;
using System.Collections.Generic;

namespace FishingCactus.Input
{
    public class InputSystem : InputSystemBase
    {
        public override void Initialize( Settings settings )
        {
            base.Initialize( settings );

            ReInput.ControllerConnectedEvent += OnControllerConnected;
            ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
            ReInput.ControllerPreDisconnectEvent += OnControllerPreDisconnect;
        }

        public override void RemoveAllGamepadsFromPlayer( int player_id )
        {
            var player = ReInput.players.GetPlayer( player_id );

            if ( player == null )
            {
                Logger.Log( LogLevel.Error, $"No rewired player found for the index {player_id}" );
                return;
            }

            for (int joystick_index = AssignedJoysticks.Count - 1; joystick_index >= 0; joystick_index-- )
            {
                if ( AssignedJoysticks[joystick_index].GamePlayerId == player_id )
                {
                    var joystick = GetJoystickFromUnityId( AssignedJoysticks[joystick_index].ControllerId );

                    if ( joystick != null )
                    {
                        player.controllers.RemoveController( joystick );
                    }

                    AssignedJoysticks.RemoveAt(joystick_index);
                }
            }

            // When there are two controllers connected from the start it can be that it does not get emptied correctly
            player.controllers.ClearControllersOfType(ControllerType.Joystick);
        }

        public override void RemoveGamepadFromPlayer( int controller_id, int player_id )
        {
            var player = ReInput.players.GetPlayer( player_id );

            if ( player == null )
            {
                Logger.Log( LogLevel.Error, $"No rewired player found for the index {player_id}" );
                return;
            }

            var joystick = GetJoystickFromUnityId( controller_id );
            if ( joystick == null )
            {
                Logger.Log( LogLevel.Warning, $"No joystick found with the id {controller_id}" );
                return;
            }

            for (int joystick_index = AssignedJoysticks.Count - 1; joystick_index >= 0; joystick_index-- )
            {
                if ( AssignedJoysticks[joystick_index].ControllerId == controller_id )
                {
                    AssignedJoysticks.RemoveAt(joystick_index);
                }
            }

#if UNITY_XBOXONE && !UNITY_EDITOR
            // This only needs to be done if there are multiple controllers connected on the same user for xbox
            player.controllers.RemoveController( joystick );
#endif
        }

        public override void AssignGamepadToPlayer( int controller_id, int player_id, IUniqueUserId unique_user_id )
        {
            var player = ReInput.players.GetPlayer( player_id );

            if ( player == null )
            {
                Logger.Log( LogLevel.Error, $"No rewired player found for the index {player_id}" );
                return;
            }

            var joystick = GetJoystickFromUnityId( controller_id );
            if ( joystick == null )
            {
                Logger.Log( LogLevel.Warning, $"No joystick found with the id {controller_id}" );
                return;
            }

            if ( AssignedJoysticks.Exists( assignment_map => assignment_map.ControllerId == controller_id ) )
            {
                Logger.Log( LogLevel.Warning, "Trying to assign a game-pad which is already assigned" );
                return;
            }

            AssignedJoysticks.Add( new GamepadAssignmentMap( player_id, controller_id, unique_user_id ) );
            player.controllers.AddController( joystick, true );
        }

        public override bool IsAnyKeyDown( out int controller_index )
        {
            IList<Joystick> joysticks = ReInput.controllers.Joysticks;

            for (int i = 0; i < joysticks.Count; i++)
            {
                Joystick joystick = joysticks[i];
                if ( joystick.GetAnyButtonDown() )
                {
                    controller_index = joystick.id;
                    return true;
                }
            }

            var keyboard = ReInput.controllers.Keyboard;
            if ( keyboard != null && keyboard.GetAnyButtonDown() )
            {
                controller_index = keyboard.id;
                return true;
            }

            var mouse = ReInput.controllers.Mouse;
            if ( mouse != null && mouse.GetAnyButtonDown() )
            {
                controller_index = mouse.id;
                return true;
            }

            controller_index = -1;
            return false;
        }

#if UNITY_XBOXONE
        public override int GetMappedControllerIdFromXboxControllerId( ulong controller_id )
        {
            foreach ( var joystick in ReInput.controllers.Joysticks )
            {
                ulong xbox_id = joystick.GetExtension<Rewired.Platforms.XboxOne.XboxOneGamepadExtension>().xboxOneJoystickId;
                if ( xbox_id == controller_id )
                {
                    return joystick.id;
                }
            }

            return -1;
        }
#endif

        private Joystick GetJoystickFromUnityId( int controller_id )
        {
            foreach ( var joystick in ReInput.controllers.Joysticks )
            {
                if ( joystick.id == controller_id )
                {
                    return joystick;
                }
            }

            return null;
        }

        private void OnControllerConnected( ControllerStatusChangedEventArgs args )
        {
            if ( args.controllerType != ControllerType.Joystick )
            {
                return;
            }

            int controller_id = ( ( Joystick ) args.controller ).id;

            InvokeOnControllerConnectionChanged( true, controller_id );
        }

        private void OnControllerDisconnected( ControllerStatusChangedEventArgs args )
        {
            if ( args.controllerType != ControllerType.Joystick )
            {
                return;
            }

            InvokeOnControllerConnectionChanged( false, args.controllerId );
        }

        private void OnControllerPreDisconnect( ControllerStatusChangedEventArgs args )
        {
        }
    }
}

#endif
