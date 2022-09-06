using FishingCactus.Setup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishingCactus.User
{
    public enum ELoginStatus
    {
        NotLoggedIn,
        UsingLocalProfile,
        LoggedIn
    }

    public enum ELoginResult
    {
        Failed,
        SuccessLocalProfile,
        SuccessOnlineProfile
    }

    public enum EUserPrivileges
    {
        CanPlay,
        CanPlayOnline,
        CanCommunicateOnline,
        CanUseUserGeneratedContent
    }

    [Flags]
    public enum EUserPrivilegesResult
    {
        NoFailures = 0,
        RequiredPatchAvailable = 1 << 0,
        RequiredSystemUpdate = 1 << 1,
        AgeRestrictionFailure = 1 << 2,
        AccountTypeFailure = 1 << 3,
        UserNotFound = 1 << 4,
        UserNotLoggedIn = 1 << 5,
        ChatRestriction = 1 << 6,
        UgcRestriction = 1 << 7,
        GenericFailure = 1 << 8,
        OnlinePlayRestricted = 1 << 9,
        NetworkConnectionUnavailable = 1 << 10
    }

    public delegate void OnLoginStatusChangedDelegate( ELoginStatus old_status, ELoginStatus new_status, IUniqueUserId new_user_id );    
    public delegate void OnControllerPairingChangedDelegate( int controller_id, IUniqueUserId previous_user_id, IUniqueUserId new_user_id );

    public struct LoginResult
    {
        public LoginResult( ELoginResult result, IUniqueUserId user_id, string error_string )
        {
            Result = result;
            UserId = user_id;
            ErrorString = error_string;
        }

        public ELoginResult Result;
        public IUniqueUserId UserId;
        public string ErrorString;
    }

    public struct GetUserAvatarResult
    {
        public bool Success;
        public Texture2D Avatar;
    }
    
    public enum AvatarSize
    {
        Small = 1,
        Medium = 2,
        Large = 3,
        ExtraLarge = 4
    }

    public interface IPlatformUserSystem
    {
        event OnLoginStatusChangedDelegate OnLoginStatusChanged;
        event OnControllerPairingChangedDelegate OnControllerPairingChanged;

        void Initialize( Settings settings );
        Task<LoginResult> Login( int controller_id );
        Task<bool> Logout( int controller_id );
        IUserOnlineAccount GetUserAccount( IUniqueUserId user_id );
        IReadOnlyList<IUserOnlineAccount> GetAllUserAccounts();
        IUniqueUserId GetUniqueUserId( int controller_id );
        IUniqueUserId CreateUniqueUserId( byte [] bytes, int size );
        IUniqueUserId CreateUniqueUserId( string textual_representation );
        ELoginStatus GetLoginStatus( IUniqueUserId user_id );
        ELoginStatus GetLoginStatus( int controller_id );
        string GetPlayerNickname( IUniqueUserId user_id );
        Task<EUserPrivilegesResult> GetUserPrivilege( IUniqueUserId user_id, EUserPrivileges privilege );
        Task<GetUserAvatarResult> GetUserAvatar(IUniqueUserId user_id, AvatarSize avatar_size);
    }
}
