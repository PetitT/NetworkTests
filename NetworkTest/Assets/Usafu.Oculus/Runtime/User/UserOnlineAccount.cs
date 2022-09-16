using System;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class UserOnlineAccount : IUserOnlineAccount
    {
        public string DisplayName { get => string.Empty; }
        public string RealName { get => string.Empty; }
        public IUniqueUserId UserId{ get => userId; }

        public UserOnlineAccount( IUniqueUserId user_id )
        {
            userId = user_id;
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