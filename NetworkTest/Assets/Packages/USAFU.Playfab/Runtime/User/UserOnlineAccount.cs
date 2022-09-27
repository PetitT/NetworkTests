using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class UserOnlineAccount : IUserOnlineAccount
    {
        public string DisplayName { get => accountData[StringConstants.DISPLAY_NAME]; }
        public string RealName { get => string.Empty; }
        public IUniqueUserId UserId { get => userId; }

        private IUniqueUserId userId;

        //Prefill all user datas on init here
        private Dictionary<string, string> accountData;

        public UserOnlineAccount( IUniqueUserId user_id )
        {
            userId = user_id;
        }

        public bool GetAuthAttributeByName( out string attribute_value, string attribute_name )
        {
            return GetUserAttribute( attribute_name, out attribute_value );
        }

        public bool GetUserAttribute( string attribute_name, out string result )
        {
            return accountData.TryGetValue( attribute_name, out result );
        }

        public bool SetUserAttributeByName( string attribute_value, string attribute_name )
        {
            accountData[attribute_name] = attribute_value;
            return true;
        }

        public Task<string> GetAccessToken( IAccessTokenRequestInfos request_infos = null )
        {
            return Task.FromResult( string.Empty );
        }
    }
}