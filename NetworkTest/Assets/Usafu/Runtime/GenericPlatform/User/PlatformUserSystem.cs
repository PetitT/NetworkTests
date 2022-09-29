using FishingCactus.Setup;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class PlatformUserSystem : IPlatformUserSystem
    {
        private static readonly UniqueUserId _singleAccountId = new UniqueUserId( 0 );
        private static readonly UserOnlineAccount _singleAccount = new UserOnlineAccount( _singleAccountId );

        public Task< LoginResult > Login( int controller_id )
        {
            return Task.FromResult( new LoginResult( ELoginResult.SuccessOnlineProfile, _singleAccountId, null ) );
        }

        public Task< bool > Logout( int controller_id )
        {
            return Task.FromResult( true );
        }

        public IUserOnlineAccount GetUserAccount( IUniqueUserId user_id )
        {
            return _singleAccount;
        }

        public IReadOnlyList< IUserOnlineAccount > GetAllUserAccounts()
        {
            return new ReadOnlyCollection< IUserOnlineAccount >( new List< IUserOnlineAccount > { _singleAccount } );
        }

        public IUniqueUserId GetUniqueUserId( int controller_id )
        {
            return _singleAccountId;
        }

        public IUniqueUserId CreateUniqueUserId( byte[] bytes, int size )
        {
            return _singleAccountId;
        }

        public IUniqueUserId CreateUniqueUserId( string textual_representation )
        {
            return _singleAccountId;
        }

        public ELoginStatus GetLoginStatus( IUniqueUserId user_id )
        {
            return GetLoginStatus( 0 );
        }
        
        public ELoginStatus GetLoginStatus( int controller_id )
        {
            return ELoginStatus.UsingLocalProfile;
        }

        public string GetPlayerNickname( IUniqueUserId user_id )
        {
            return null;
        }

        public Task< EUserPrivilegesResult > GetUserPrivilege( IUniqueUserId user_id, EUserPrivileges privilege )
        {
            return Task.FromResult( EUserPrivilegesResult.NoFailures );
        }

        public void Initialize( Settings settings )
        {
        }

        public Task<GetUserAvatarResult> GetUserAvatar(IUniqueUserId user_id, AvatarSize avatar_size)
        {
            return Task.FromResult(new GetUserAvatarResult { Success = false, Avatar = null });
        }

#pragma warning disable CS0067 // The event 'event' is never used
        public event OnControllerPairingChangedDelegate OnControllerPairingChanged;
        public event OnLoginStatusChangedDelegate OnLoginStatusChanged;
#pragma warning restore CS0067 // The event 'event' is never used
    }
}
