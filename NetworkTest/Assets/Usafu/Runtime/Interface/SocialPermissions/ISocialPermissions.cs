using FishingCactus.Setup;
using FishingCactus.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FishingCactus.SocialPermissions
{
    public enum ECommunicateWithUserPermission
    {
        CommunicateWithText,
        CommunicateWithVoice
    }

    public interface IOnlineBlockedUser : IOnlineUser
    {}

    public interface IOnlineMutedUser : IOnlineUser
    {}

    public enum ECommunicationPermissionResult
    {
        Available,
        AvailableIfFriendsInGame,
        NotAvailable
    }

    public struct CommunicationPermissionResult
    {
        public ECommunicationPermissionResult Permission;
        public string UserId;
    }

    public interface ISocialPermissions
    {
        void Initialize( Settings platform_settings );
        Task<bool> QueryBlockedUsers( IUniqueUserId user_id );
        bool IsUserBlocked( IUniqueUserId user_id, IUniqueUserId other_user_id );
        IEnumerable<IOnlineBlockedUser> GetBlockedUsers( IUniqueUserId user_id );
        Task<bool> QueryMutedUsers( IUniqueUserId user_id );
        bool IsUserMuted( IUniqueUserId user_id, IUniqueUserId other_user_id );
        IEnumerable<IOnlineMutedUser> GetMutedUsers( IUniqueUserId user_id );

        Task<CommunicationPermissionResult> CanCommunicateWithUser( IUniqueUserId user_id, string other_user_id, string other_platform_name, ECommunicateWithUserPermission permission );
        void ResetCaches();
    }

    public class SocialPermissionsBase : ISocialPermissions
    {
        public virtual void Initialize( Settings platform_settings )
        {
            USAFUCore.Get().Platform.OnResumeApplication += ( time_suspended ) => ResetCaches();
        }

        public virtual Task<bool> QueryBlockedUsers( IUniqueUserId user_id )
        {
            return Task.FromResult( true );
        }

        public virtual bool IsUserBlocked( IUniqueUserId user_id, IUniqueUserId other_user_id )
        {
            if ( blockedUsersMap.TryGetValue( user_id, out List< IOnlineBlockedUser > blocked_users ) )
            {
                foreach ( var blocked_user in blocked_users )
                {
                    if ( blocked_user.UserId.Equals( other_user_id ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual IEnumerable<IOnlineBlockedUser> GetBlockedUsers( IUniqueUserId user_id )
        {
            if ( blockedUsersMap.TryGetValue( user_id, out List< IOnlineBlockedUser > result ) )
            {
                return result;
            }

            return Enumerable.Empty<IOnlineBlockedUser>();
        }

        public virtual Task<bool> QueryMutedUsers( IUniqueUserId user_id )
        {
            return Task.FromResult( true );
        }

        public virtual bool IsUserMuted( IUniqueUserId user_id, IUniqueUserId other_user_id )
        {
            if ( mutedUsersMap.TryGetValue( user_id, out List< IOnlineMutedUser > muted_users ) )
            {
                foreach ( var muted_user in muted_users )
                {
                    if ( muted_user.UserId.Equals( other_user_id ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual IEnumerable<IOnlineMutedUser> GetMutedUsers( IUniqueUserId user_id )
        {
            if ( mutedUsersMap.TryGetValue( user_id, out List< IOnlineMutedUser > result ) )
            {
                return result;
            }

            return Enumerable.Empty<IOnlineMutedUser>();
        }

        public virtual Task<CommunicationPermissionResult> CanCommunicateWithUser( IUniqueUserId user_id, string other_user_id, string other_platform_name, ECommunicateWithUserPermission permission )
        {
            return Task.FromResult( new CommunicationPermissionResult { UserId = other_user_id, Permission = ECommunicationPermissionResult.Available } );
        }

        public virtual void ResetCaches()
        {
            blockedUsersMap.Clear();
            mutedUsersMap.Clear();
            communicationPermissionsMap.Clear();
        }

        protected List< IOnlineBlockedUser > GetOrCreateBlockedUsers( IUniqueUserId user_id )
        {
            List< IOnlineBlockedUser > blocked_users;

            if ( !blockedUsersMap.TryGetValue( user_id, out blocked_users ) )
            {
                blocked_users = new List<IOnlineBlockedUser>();
                blockedUsersMap.Add( user_id, blocked_users );
            }

            return blocked_users;
        }

        protected List< IOnlineMutedUser > GetOrCreateMutedUsers( IUniqueUserId user_id )
        {
            List< IOnlineMutedUser > muted_users;

            if ( !mutedUsersMap.TryGetValue( user_id, out muted_users ) )
            {
                muted_users = new List<IOnlineMutedUser>();
                mutedUsersMap.Add( user_id, muted_users );
            }

            return muted_users;
        }

        protected Dictionary< string, Dictionary< ECommunicateWithUserPermission, ECommunicationPermissionResult > > GetOrCreateCommunicationPermissionMap( IUniqueUserId user_id )
        {
            Dictionary< string, Dictionary< ECommunicateWithUserPermission, ECommunicationPermissionResult > > result;

            if ( !communicationPermissionsMap.TryGetValue( user_id, out result ) )
            {
                result = new Dictionary< string, Dictionary< ECommunicateWithUserPermission, ECommunicationPermissionResult > >();
                communicationPermissionsMap.Add( user_id, result );
            }

            return result;
        }

        protected readonly Dictionary< IUniqueUserId, Dictionary< string, Dictionary< ECommunicateWithUserPermission, ECommunicationPermissionResult > > > communicationPermissionsMap = new Dictionary<IUniqueUserId, Dictionary<string, Dictionary<ECommunicateWithUserPermission, ECommunicationPermissionResult>>>();
        protected readonly Dictionary< IUniqueUserId, List< IOnlineBlockedUser > > blockedUsersMap = new Dictionary< IUniqueUserId, List< IOnlineBlockedUser > >();
        protected readonly Dictionary< IUniqueUserId, List< IOnlineMutedUser > > mutedUsersMap = new Dictionary< IUniqueUserId, List< IOnlineMutedUser > >();
    }
}