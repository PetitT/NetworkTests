using System;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class UserOnlineAccount : IUserOnlineAccount
    {
        public string DisplayName { get => _displayName; }
        public string RealName { get => string.Empty; }
        public IUniqueUserId UserId{ get => userId; }

        private string _displayName;

        public UserOnlineAccount( IUniqueUserId user_id )
        {
            userId = user_id;
        }

        public UserOnlineAccount( IUniqueUserId user_id, string display_name)
        {
            userId = user_id;
            _displayName = display_name;
        }

        public Task< string > GetAccessToken( IAccessTokenRequestInfos request_infos = null )
        {
            return Task.FromResult( string.Empty );
        }

        public bool GetAuthAttributeByName( out string attribute_value, string attribute_name )
        {
            throw new NotImplementedException();
        }

        public bool GetUserAttribute( string attribute_name, out string result )
        {
            throw new NotImplementedException();
        }

        public bool SetUserAttributeByName( string attribute_value, string attribute_name )
        {
            throw new NotImplementedException();
        }

        private IUniqueUserId userId;
    }
}