using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishingCactus;
using FishingCactus.ExternalUI;
using FishingCactus.User;
using System.Threading.Tasks;
using System;

namespace FishingCactus
{
    public static class LoginFlow
    {
        public enum LoginFailedReason
        {
            InvalidUser,
            InvalidPrivilegeUserNotLoggedIn,
            InvalidPrivilegeAgeRestriction,
            InvalidPrivilegeOther,
            InvalidResultFromExternalUI,
            InvalidLicense
        }

        public static bool isHandlingLoginFlow { get; private set; }
        public static bool isHandlingLoginSuccess { get; private set; }

        private static Action<IUniqueUserId, int> onLoginSuccessful;
        private static Action<LoginFailedReason, int> onLoginFailed;

        /// <summary>
        /// Detect input for any user.
        /// </summary>
        /// <param name="controller_id">the controller id that was pressed.</param>
        /// <returns>indicates if a user input was detected.</returns>
        public static bool DetectInput( out int controller_id )
        {
            if ( LoginFlow.isHandlingLoginFlow )
            {
                controller_id = -1;
                return false;
            }

            return USAFUCore.Get().InputSystem.IsAnyKeyDown( out controller_id );
        }

        /// <summary>
        /// Attempts the login flow. 
        /// </summary>
        /// <param name="controller_id">the controller id that triggered the login attempt.</param>
        /// <param name="on_login_succesful">callback when login was succesful.</param>
        /// <param name="on_login_failed">callback when login failed.</param>
        public static async void TryLogin( int controller_id, Action<IUniqueUserId, int> on_login_successful, Action<LoginFailedReason, int> on_login_failed )
        {
            if ( isHandlingLoginFlow )
            {
                return;
            }
            Debug.Log( $"Trying to login controller: {controller_id}" );

            isHandlingLoginFlow = true;
            onLoginFailed = on_login_failed;
            onLoginSuccessful = on_login_successful;

            bool is_game_licensed = true;
            bool can_continue_to_main_menu = true;

            try
            {

                if ( USAFUCore.Get().UserSystem.GetLoginStatus( controller_id ) == ELoginStatus.NotLoggedIn || !is_game_licensed )
                {
                    Debug.Log( "Preparing to show login UI" );
                    ShowLoginUiResult show_login_ui_result = await USAFUCore.Get().ExternalUI.ShowLoginUi( controller_id, false );

                    if ( !show_login_ui_result.Success )
                    {
                        can_continue_to_main_menu = false;
                    }
                }
            }
            catch ( Exception e )
            {
                Debug.LogException( e );
                can_continue_to_main_menu = false;
            }

            if ( can_continue_to_main_menu )
            {
                Debug.Log( "Preparing to close Login UI" );
                IUniqueUserId unique_id = USAFUCore.Get().UserSystem.GetUniqueUserId( controller_id );
                await HandleLoginUIClosed( unique_id, controller_id );
            }
            else
            {
                Debug.Log( "User cannot play" );
                OnUserCannotPlay( LoginFailedReason.InvalidResultFromExternalUI, controller_id );
            }
        }

        private static async Task HandleLoginUIClosed( IUniqueUserId unique_user_id, int controller_id )
        {
            if ( !unique_user_id.IsValid )
            {
                Debug.Log( "Invaid User ID. User cannot play" );
                OnUserCannotPlay( LoginFailedReason.InvalidUser, controller_id );
                return;
            }

            // :TODO:
            bool is_game_licensed = true;

            if ( !is_game_licensed )
            {
                Debug.Log( "Game not licensed. User cannot play." );
                OnUserCannotPlay( LoginFailedReason.InvalidLicense, controller_id );
                return;
            }

            EUserPrivilegesResult user_privileges = await USAFUCore.Get().UserSystem.GetUserPrivilege( unique_user_id, EUserPrivileges.CanPlay );

            if ( user_privileges != EUserPrivilegesResult.NoFailures )
            {
                Debug.Log( "Incorrect user privilege. User cannot play." );
                if ( user_privileges.HasFlag( EUserPrivilegesResult.UserNotLoggedIn ) )
                {
                    OnUserCannotPlay( LoginFailedReason.InvalidPrivilegeUserNotLoggedIn, controller_id );
                }
                else if ( user_privileges.HasFlag( EUserPrivilegesResult.AgeRestrictionFailure ) )
                {
                    OnUserCannotPlay( LoginFailedReason.InvalidPrivilegeAgeRestriction, controller_id );
                }
                else
                {
                    OnUserCannotPlay( LoginFailedReason.InvalidPrivilegeOther, controller_id );
                }
                return;
            }

            await TryConnectOnlineInterface( controller_id );

        }

        private static async Task TryConnectOnlineInterface( int controller_id )
        {
            isHandlingLoginSuccess = false;

            Debug.Log( "Trying to Login with Online Interface" );
            var login_result = await USAFUCore.Get().UserSystem.Login( controller_id );
            if ( login_result.Result == ELoginResult.Failed )
            {
                Debug.LogError( $"Error when logging in: {login_result.ErrorString}" );
                OnUserCannotPlay( LoginFailedReason.InvalidUser, controller_id );
            }
            else
            {
                Debug.Log( $"Success. Login Succeeded with {login_result.Result}" );
                OnLoginSucceeded( controller_id, login_result.UserId );
            }
        }

        private static void OnLoginSucceeded( int controller_id, IUniqueUserId user_id )
        {
            // Some online interfaces (Steam for example) call OnLoginCompleteDelegate directly in Login, and return immediately.
            // If for some reason its not possible to connect to the OSS, we end up here twice. Don't allow that, using that flag
            if ( isHandlingLoginSuccess )
            {
                return;
            }

            isHandlingLoginSuccess = true;
            isHandlingLoginFlow = false;

            USAFUCore.Get().ApplicationLifeCycle.AddActionToThreadDispatcher( () =>
            {
                Debug.Log( "Calling onLoginSuccessful" );
                onLoginSuccessful?.Invoke( user_id, controller_id );
                onLoginSuccessful = null;
                onLoginFailed = null;
            } );
        }

        private static void OnUserCannotPlay( LoginFailedReason reason, int controller_id )
        {
            Debug.Log( "Calling onLoginFailed" );
            isHandlingLoginFlow = false;
            USAFUCore.Get().ApplicationLifeCycle.AddActionToThreadDispatcher( () =>
            {
                onLoginFailed?.Invoke( reason, controller_id );
                onLoginFailed = null;
                onLoginSuccessful = null;
            } );
        }
    }
}