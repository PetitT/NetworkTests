using FishingCactus;
using FishingCactus.ExternalUI;
using FishingCactus.OnlineSessions;
using FishingCactus.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private UnityEngine.UI.Button [] Buttons;
    public UnityEngine.UI.Text log;
    public UnityEngine.UI.Text isInSessionText;
    public UnityEngine.UI.Text createdSessionId;
    public UnityEngine.UI.Text enteredSessionId;
    public UnityEngine.UI.Image networkImage;
    public UnityEngine.UI.Text controllerIdText;
    public UnityEngine.UI.Text isGamePadConnectedText;
    public UnityEngine.UI.Text signInChangedText;

    private bool isLoggingIn = false;
    private bool isLoggedIn = false;
    private bool isInSession = false;
    private IUniqueUserId userId;
    private int ControllerIndex
    {
        get => controllerIndex;
        set
        {
            controllerIndex = value;
            controllerIdText.text = $"ControllerId : {value}";
        }
    }
    private int controllerIndex = -1;

    private bool IsGamepadConnected
    {
        get => isGamePadConnected;
        set
        {
            isGamePadConnected = value;
            isGamePadConnectedText.text = $"Gamepad Connected : {value}";
        }
    }
    private bool isGamePadConnected = false;

    private bool MustHandleSignInChanged
    {
        get => mustHandleSignInChanged;
        set
        {
            mustHandleSignInChanged = value;
            signInChangedText.gameObject.SetActive( value );
            Debug.Log( $"MustHandleSignInChanged : {value}" );
        }
    }
    private bool mustHandleSignInChanged = false;

    private OnlineSessionSearchResult pendingSessionSearchResult;

    public static NewBehaviourScript Instance { get; private set; }

    public void Awake()
    {
        Instance = this;

        Buttons = transform.GetComponentsInChildren<UnityEngine.UI.Button>();

        ToggleButtons( false );

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        USAFUCore.Get().OnlineSessions.OnSessionUserInviteAccepted += OnlineSessions_OnSessionUserInviteAccepted;
        USAFUCore.Get().OnlineSessions.OnJoinSessionComplete += OnlineSessions_OnJoinSessionComplete;
        USAFUCore.Get().OnlineSessions.OnDestroySessionComplete += OnlineSessions_OnDestroySessionComplete;

        USAFUCore.Get().ApplicationLifeCycle.OnResumeApplication += ApplicationLifeCycle_OnResumeApplication;
        USAFUCore.Get().ApplicationLifeCycle.OnSuspendApplication += ApplicationLifeCycle_OnSuspendApplication;

        SetIsInSession( false );

        NetworkUtilities.OnNetworkAcquired += NetworkUtilities_OnNetworkAcquired;
        NetworkUtilities.OnNetworkLost += NetworkUtilities_OnNetworkLost;
        
        NetworkUtilities.RegisterToPlatformEvents();

        ControllerIndex = -1;
        IsGamepadConnected = true;
        MustHandleSignInChanged = false;

        Debug.Log("ApplicationManager.Awake");
    }

    private void ApplicationLifeCycle_OnSuspendApplication()
    {
        Debug.Log("ApplicationManager.OnApplicationSuspended");
    }

    private void ApplicationLifeCycle_OnResumeApplication(double parDuration)
    {
        Debug.Log("ApplicationManager.OnApplicationResumed");

        if (USAFUCore.Get().UserSystem.GetLoginStatus( userId ) != ELoginStatus.LoggedIn)
        {
            Debug.Log("ApplicationManager.OnApplicationResumed - call HandleSignInChanged");
            HandleSignInChanged();
        }
    }

    private void OnControllerConnectionChanged(bool parIsConnected, int parControllerIndex)
    {
        Debug.Log($"ApplicationManager.OnControllerConnectionChanged - parIsConnected {parIsConnected} - parControllerIndex {parControllerIndex}");
        if (USAFUCore.Get().Platform.IsConsole  )
        {
            // Display the popup only if the used controller is updated or if any controller has been connected
            if ( parControllerIndex == controllerIndex 
                || parIsConnected )
            {
                IsGamepadConnected = parIsConnected;
            }
        }
    }

    private void HandleSignInChanged()
    {
        Debug.Log($"ApplicationManager.HandleSignInChanged");

        LogText( "SIGN_IN_CHANGED" );

        MustHandleSignInChanged = true;
    }

    // This function can only be called on XB1
    private void OnControllerPairingChanged(int parControllerIndex, IUniqueUserId parPreviousUserId, IUniqueUserId parNewUserId)
    {
        bool locPreviousUserIsValid = parPreviousUserId != null && parPreviousUserId.IsValid;
        bool locNewUserIsValid = parNewUserId != null && parNewUserId.IsValid;

        Debug.Log($"ApplicationManager.OnControllerPairingChanged - parControllerIndex : {parControllerIndex} - locPreviousUserIsValid : {locPreviousUserIsValid} - locNewUserIsValid : {locNewUserIsValid}");

        if (locPreviousUserIsValid && !locNewUserIsValid)
        {
            Debug.Log($"ApplicationManager.OnControllerPairingChanged - #1" );
            // Treat this as a disconnect or sign out, which is handled somewhere else
            return;
        }

        if ( !locPreviousUserIsValid && locNewUserIsValid )
        {
            Debug.Log($"ApplicationManager.OnControllerPairingChanged - #2" );
            if ( parNewUserId.Equals( userId ) )
            {
                // Same user reconnected (probably its gamepad reconnected)
                IsGamepadConnected = true;
                signInChangedText.gameObject.SetActive( false );

                Debug.Log($"ApplicationManager.OnControllerPairingChanged - #2 - parNewUserId.Equals( uniqueUserId ) - Set ControllerIndex to {parControllerIndex}" );
                ControllerIndex = parControllerIndex;
                return;
            }
            else
            {
                // Treat this as a signin, if it is from a different controller to a different user than the current user it should not interrupt the game.
                Debug.Log($"ApplicationManager.OnControllerPairingChanged - #2 - NOT parNewUserId.Equals( uniqueUserId ) - HandleSignInChanged" );
                HandleSignInChanged();
                return;
            }
        }

        if (locPreviousUserIsValid && locNewUserIsValid)
        {
            Debug.Log($"ApplicationManager.OnControllerPairingChanged - #3" );
            // This is switching a profile and should not trigger anything except if it is the active user
            if ( ControllerIndex == parControllerIndex && !parNewUserId.Equals( userId ) )
            {
                Debug.Log($"ApplicationManager.OnControllerPairingChanged - #3 - controllerIndex == parControllerIndex && !parNewUserId.Equals( uniqueUserId )" );
                HandleSignInChanged();
            }
        }
    }

    private void OnLoginStatusChanged(ELoginStatus parOldStatus, ELoginStatus parNewStatus, IUniqueUserId parUniqueUserId)
    {
        Debug.Log($"ApplicationManager.OnLoginStatusChanged - userId : {userId} - parUniqueUserId : {parUniqueUserId}" ); 
        if ( parUniqueUserId.Equals( userId ) )
        {
            Debug.Log($"ApplicationManager.OnLoginStatusChanged - parUniqueUserId == uniqueUserId" ); 
            if ( parNewStatus == ELoginStatus.NotLoggedIn )
            {
                Debug.Log($"ApplicationManager.OnLoginStatusChanged - parUniqueUserId == uniqueUserId - parNewStatus == ELoginStatus.NotLoggedIn" ); 
                HandleSignInChanged();
            }
            else
            {
                Debug.Log($"ApplicationManager.OnLoginStatusChanged - parUniqueUserId == uniqueUserId - parNewStatus != ELoginStatus.NotLoggedIn => Should cancel SignInChanged" ); 
                MustHandleSignInChanged = false;
            }
        }
    }

    private void NetworkUtilities_OnNetworkLost()
    {
        networkImage.color = Color.red;
    }

    private void NetworkUtilities_OnNetworkAcquired()
    {
        networkImage.color = Color.green;
    }

    private async Task GetToken()
    {
        if ( userId != null && userId.IsValid )
        { 
            var new_token = await USAFUCore.Get().UserSystem.GetUserAccount( userId ).GetAccessToken();

            Debug.Log( $"New token : {new_token}" );
        }
        else
        {
            Debug.LogError( $"Cannot get token" );
        }
    }

    public async void DisplayKeyboard()
    {
        enteredSessionId.text = await USAFUCore.Get().PlatformInput.GetVirtualKeyboardTextAsync( enteredSessionId.text, "Enter a session id", string.Empty );
    }

    private void OnlineSessions_OnDestroySessionComplete( string session_name, bool success )
    {
        SetIsInSession( !success );
        LogText( "Session destroyed" );
    }

    private void OnlineSessions_OnJoinSessionComplete( string session_name, JoinSessionCompleteResult result )
    {
        SetIsInSession( result == JoinSessionCompleteResult.Success );
        isInSessionText.text += $" {result}";
    }

    private async void OnlineSessions_OnSessionUserInviteAccepted( bool success, IUniqueUserId user_id, OnlineSessionSearchResult session_search_result )
    {
        LogText( $"OnlineSessions_OnSessionUserInviteAccepted : {success}" );

        if ( isLoggedIn )
        {
            await JoinSessionFromSessionSearchResult( session_search_result );
        }
        else
        {
            pendingSessionSearchResult = session_search_result;
        }
    }

    private void LogText( string text )
    {
        log.text = text;
    }

    public void Update()
    {
        if ( isLoggingIn 
            || isLoggedIn
            )
        {
            return;
        }

        int controller_index = 0;
        if ( USAFUCore.Get().InputSystem.IsAnyKeyDown( out controller_index ) )
        {
            Login( controller_index );
        }
    }

    private async Task Login( int controller_index )
    {
        //ConsoleUtils.ConsoleUtilsManager.TitleIdHex();

        isLoggingIn = true;

        LogText( "Loging in" );

        if ( !USAFUCore.Get().Platform.IsInitialized )
        {
            OnUserCannotPlay();
            return;
        }

        bool locIsGameLicensed = true;
        bool locCanMoveToMainMenu = true;

        if ( USAFUCore.Get().UserSystem.GetLoginStatus( controller_index ) == ELoginStatus.NotLoggedIn
            || !locIsGameLicensed )
        {
            ShowLoginUiResult locShowLoginUiResult = await USAFUCore.Get().ExternalUI.ShowLoginUi( controller_index, false );

            if ( !locShowLoginUiResult.Success )
            {
                locCanMoveToMainMenu = false;
            }
        }

        if ( locCanMoveToMainMenu )
        {
            IUniqueUserId locUniqueId = USAFUCore.Get().UserSystem.GetUniqueUserId( controller_index );

            await HandleLoginUiClosed( locUniqueId, controller_index );
        }
        else
        {
            OnUserCannotPlay();
        }
    }

    private async Task HandleLoginUiClosed( IUniqueUserId parUniqueUserId, int parControllerIndex )
    {
        if ( !parUniqueUserId.IsValid )
        {
            OnUserCannotPlay();
            return;
        }

        // :TODO:
        bool locIsGameLicensed = true;

        if ( !locIsGameLicensed )
        {
            // :TODO: Show popup
            OnUserCannotPlay();
            return;
        }

        EUserPrivilegesResult locUserPrivileges = await USAFUCore.Get().UserSystem.GetUserPrivilege( parUniqueUserId, EUserPrivileges.CanPlay );

        if ( locUserPrivileges != EUserPrivilegesResult.NoFailures )
        {
            OnUserCannotPlay();
            return;
        }

        /* :TODO: Call this code on demand when trying to play an online game*/
        locUserPrivileges = await USAFUCore.Get().UserSystem.GetUserPrivilege( parUniqueUserId, EUserPrivileges.CanPlayOnline );

        if ( locUserPrivileges != EUserPrivilegesResult.NoFailures )
        {
            OnUserCannotPlay();
            return;
        }

        await TryConnectOnlineInterface( parControllerIndex );
    }

    private async Task TryConnectOnlineInterface( int parControllerIndex )
    {
        try
        {
            LoginResult locLoginResult = await USAFUCore.Get().UserSystem.Login( parControllerIndex );

            if ( locLoginResult.Result != ELoginResult.SuccessOnlineProfile )
            {
                Debug.LogWarning( $"Error when login in : {locLoginResult.ErrorString}" );
                OnUserCannotPlay();
            }
            else
            {
                await OnLoginSucceeded( parControllerIndex, locLoginResult.UserId );
            }
        }
        catch ( Exception e )
        {
            Debug.LogWarning( $"Cannot login: {e.Message}" );
            OnUserCannotPlay();
        }
    }

    private async Task OnLoginSucceeded( int parControllerIndex, IUniqueUserId parUniqueUserId )
    {
        enabled = false;
        isLoggedIn = true;

        userId = parUniqueUserId;
        ControllerIndex = parControllerIndex;

        ToggleButtons( true );

        Buttons [ 0 ].Select();

        LogText( "Successfully logged in" );

        isLoggingIn = false;

        if ( pendingSessionSearchResult != null )
        {
            await JoinSessionFromSessionSearchResult( pendingSessionSearchResult );
            pendingSessionSearchResult = null;
        }

        USAFUCore.Get().ApplicationLifeCycle.OnControllerConnectionChanged += OnControllerConnectionChanged;
        USAFUCore.Get().ApplicationLifeCycle.OnControllerPairingChanged += OnControllerPairingChanged;
        USAFUCore.Get().ApplicationLifeCycle.OnLoginStatusChanged += OnLoginStatusChanged;

        //await GetToken();
    }

    public void PrintNetworkStatus()
    { 
        Debug.Log( $"IsNetworkAvailable {USAFUCore.Get().Platform.IsNetworkAvailable}" );
    }

    private async Task< bool > JoinSessionFromSessionSearchResult( OnlineSessionSearchResult search_result )
    {
        try
        {
            ToggleButtons( false );

            if ( await USAFUCore.Get().OnlineSessions.JoinSession( userId, "LOBBY_NAME", search_result ) )
            {
                LogText( "JoinSession. Could join session" );
                SetIsInSession( true );
                return true;
            }
            else
            {
                LogText( "JoinSession. Could NOT join session" );
                SetIsInSession( false );
            }
        }
        catch ( Exception e )
        {
            LogText( $"JoinSession. ERROR {e.Message}" );
        }
        finally
        {
            ToggleButtons( true );
        }

        return false;
    }

    public async void JoinSessionById()
    {
        if ( string.IsNullOrEmpty( enteredSessionId.text ) )
        {
            LogText( "JoinSessionById. Empty session id" );
            return;
        }

        try
        {
            ToggleButtons( false );

            var result = await USAFUCore.Get().OnlineSessions.FindSessionById( userId, enteredSessionId.text, null );

            if ( !result.Item1 )
            {
                LogText( "JoinSessionById. Could not find an online session with that ID" );
                return;
            }

            await JoinSessionFromSessionSearchResult( result.Item2 );
        }
        catch ( Exception e )
        {
            LogText( $"JoinSessionById. ERROR {e.Message}" );
        }
        finally
        {
            ToggleButtons( true );
        }
    }

    public async void JoinFriendSession()
    {
        try
        {
            ToggleButtons( false );

            if ( !await USAFUCore.Get().Friends.ReadFriendsList( userId, "tryCached" ) )
            {
                LogText( "JoinFriendSession failed to read friends list" );
                return;
            }

            var friends_list = USAFUCore.Get().Friends.GetFriendsList( userId, "tryCached" );

            if ( friends_list.Count == 0 )
            {
                LogText( "JoinFriendSession. Empty friends list" );
                return;
            }

            foreach ( var friend in friends_list )
            {
                var result = await USAFUCore.Get().OnlineSessions.FindFriendSession( userId, friend.UserId );

                if ( result.Item1 )
                {
                    if ( await JoinSessionFromSessionSearchResult( result.Item2 ) )
                    {
                        return;
                    }
                }
            }

            LogText( "JoinFriendSession. Could not find an online session for a friend" );
        }
        finally
        {
            ToggleButtons( true );
        }
    }

    public async void DestroySession()
    {
        if ( !isLoggedIn )
        {
            LogText( "Cannot create session if not logged in" );
            return;
        }

        LogText( "Destroying session" );
        ToggleButtons( false );

        var result = await USAFUCore.Get().OnlineSessions.DestroySession( "LOBBY_NAME" );

        LogText( $"Destroyed session? {result}" );
        ToggleButtons( true );

        SetIsInSession( !result );
    }

    public async void CreateSession()
    {
        if ( !isLoggedIn )
        {
            LogText( "Cannot create session if not logged in" );
            return;
        }

        LogText( "Creating session" );
        ToggleButtons( false );

        bool locMakePublic =
            #if UNITY_PS4
                false;
            #else
                true;
            #endif

        var locSessionSettings = new OnlineSessionSettings
        {
            NumPublicConnections = 0,
            NumPrivateConnections = 8,
            UsesPresence = true,
            AllowInvites = locMakePublic,
            AllowJoinViaPresence = locMakePublic,
            IsDedicated = true,
            AllowJoinInProgress = locMakePublic,
            ShouldAdvertise = true
        };

        locSessionSettings.Settings.Add( "SESSIONTEMPLATENAME", new OnlineSessionSetting
        {
            Data = "FriendlyGameSession",
            AdvertisementType = OnlineDataAdvertisementType.DontAdvertise
        } );

        locSessionSettings.Settings.Add( "STATUS", new OnlineSessionSetting
        {
            Data = "FaeriaGame",
            AdvertisementType = OnlineDataAdvertisementType.DontAdvertise
        } );

        if ( await USAFUCore.Get().OnlineSessions.CreateSession( userId, "LOBBY_NAME", locSessionSettings ) )
        {
            LogText( "Created session - Starting it" );
            if ( await USAFUCore.Get().OnlineSessions.StartSession( "LOBBY_NAME" ) )
            {
                LogText( "Successfully created and started the session" );

                var named_session = USAFUCore.Get().OnlineSessions.GetNamedSession( "LOBBY_NAME" );

                createdSessionId.text = named_session.SessionIdStr;

                SetIsInSession( true );
            }
            else
            {
                LogText( "Failed to start the session" );
            }
        }
        else
        {
            LogText( "Failed to create the session" );
        }

        ToggleButtons( true );
    }

    public async void SendInviteWithDialog()
    {
        await USAFUCore.Get().ExternalUI.ShowInviteUI( userId, "LOBBY_NAME" );
    }

    public async void SendInviteToAllFriends()
    {
        if ( !await USAFUCore.Get().Friends.ReadFriendsList( userId, "tryCached" ) )
        {
            LogText( "JoinFriendSession failed to read friends list" );
            return;
        }

        var online_friends = USAFUCore.Get().Friends.GetFriendsList( userId, "tryCached" );

        if ( online_friends.Count == 0 )
        {
            LogText( "No friends" );
            return;
        }

        List<IUniqueUserId> friend_ids = new List<IUniqueUserId>();

        foreach ( var friend in online_friends )
        {
            friend_ids.Add( friend.UserId );
        }

        if ( await USAFUCore.Get().OnlineSessions.SendSessionInviteToFriends( userId, friend_ids, "LOBBY_NAME" ) )
        {
            LogText( "Send the invitation successfully" );
        }
        else
        {
            LogText( "Failed to send the invitations" );
        }
    }

    private void OnUserCannotPlay()
    {
        LogText( "Error while logging in" );

        isLoggingIn = false;
    }

    private void ToggleButtons( bool enabled )
    {
        foreach ( var button in Buttons )
        {
            button.interactable = enabled;
        }

        if ( enabled )
        {
            Buttons [ 0 ].Select();
        }
    }

    private void SetIsInSession( bool is_in_session )
    {
        isInSession = is_in_session;
        isInSessionText.text = isInSession ? "IS IN SESSION" : "NOT IN SESSION";
        if ( !is_in_session )
        {
            createdSessionId.text = string.Empty;
        }
    }
}
