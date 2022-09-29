using FishingCactus.Util;
using System;
using UnityEngine;

namespace FishingCactus.Setup
{
    [Flags]
    public enum PushNotificationsFlags
    {
        /// <summary>Indicates if the application wishes to receive a notification when a game data message is received</summary>
        NewGameDataMessage = ( 1 << 0 ),
        /// <summary>Indicates if the application wishes to receive a notification when an invitation is received</summary>
        NewInvitation = ( 1 << 1 ),
        /// <summary>Indicates if the application wishes to receive a notification when the blocked users list is modified</summary>
        UpdateBlockedUsersList = ( 1 << 2 ),
        /// <summary>Indicates if the application wishes to receive a notification when the presence information of a friend is modified</summary>
        UpdateFriendPresence = ( 1 << 3 ),
        /// <summary>Indicates if the application wishes to receive a notification when the friends list is modified</summary>
        UpdateFriendsList = ( 1 << 4 ),
        /// <summary>Indicates if the application wishes to receive a notification when new in-game messages are received</summary>
        /// <remarks>This is also required to use the <see cref="Messaging.SendInGameMessage"/> method, otherwise it will result in error SCE_NP_IN_GAME_MESSAGE_ERROR_LIB_CONTEXT_NOT_FOUND</remarks>
        NewInGameMessage = ( 1 << 5 ),
    }

    [Serializable]
    public class Settings : ScriptableObject
    {
        public bool UseAchievements;
        public bool UseSaveSystem;
        public bool UseOnlineSessions;
        public bool UseOnlineLeaderboards;
        public bool RequiresOnlineServiceConnection;

        public string SaveFileExtensionWithDot = ".sav";

        public SettingsEpicGameStore EpicGameStore;
        public SettingsXB1 XB1;
        public SettingsPS4 PS4;
        public SettingsSwitch Switch;
        public SettingsStadia Stadia;
        public SettingsSteam Steam;
        public SettingsWSA WSA;
        public SettingsGoG GoG;
        public SettingsOculus Oculus;
        public SettingsGeneric Generic;
        public SettingsPlayFab PlayFab;
    }

    [Serializable]
    public struct SettingsEpicGameStore
    {
        public string productId;
        public string sandboxId;
        public string deploymentId;
        public string clientId;
        public string clientSecret;

        public string[] addOns;
    }

    [Serializable]
    public struct SettingsXB1
    {
        public string LoginXSTSEndpoint;
        public bool UseCrossNetworkTextPermission;
        public string SecureDeviceAssociationTemplateName;
    }

    [Serializable]
    public struct SettingsPS4
    {
        public string TitleId;
        public bool RequiresPSNPlus;
        [EnumFlags] public PushNotificationsFlags PushNotificationFlags;
        public int DefaultAgeRestriction;
    }

    [Serializable]
    public struct SettingsSwitch
    {
        public string ApplicationId;
        public bool MountCacheStorage;
    }

    [Serializable]
    public struct SettingsStadia
    {
        public string ApplicationId;
    }

    [Serializable]
    public struct SettingsSteam
    {
        public uint AppId;
        public bool SuspendOnUnfocus;
    }

    [Serializable]
    public struct SettingsWSA
    {
        public string LoginXSTSEndpoint;
    }

    [Serializable]
    public struct SettingsGeneric
    {
    }

    [Serializable]
    public struct SettingsGoG
    {
        public string ClientId;
        public string ClientSecret;
        public bool SuspendOnUnfocus;
    }

    [Serializable]
    public struct SettingsOculus
    {
        public string ApplicationId;
        public string DestinationApiName;
    }

    [Serializable]
    public struct SettingsPlayFab
    {
        public bool ConnectWithDevice;
    }
}