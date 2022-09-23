using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class UserOnlineAccount : IUserOnlineAccount
    {
        public const string DISPLAY_NAME = "display_name";
        public string DisplayName { get => GetDisplayName(); }
        public string RealName { get => string.Empty; }
        public IUniqueUserId UserId{ get => userId; }

        private IUniqueUserId userId;
        private Dictionary<string, string> userAttributes = new Dictionary<string, string>();

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
            return userAttributes.TryGetValue( attribute_name, out result );
        }

        private string GetDisplayName()
        {
            if(userAttributes.TryGetValue(DISPLAY_NAME, out string name ) )
            {
                return name;
            }
            else
            {
                return "";
            }
        }

        public bool SetUserAttributeByName( string attribute_value, string attribute_name )
        {
            userAttributes[attribute_name] = attribute_value;
            return true;
        }


        public Task< string > GetAccessToken( IAccessTokenRequestInfos request_infos = null )
        {           
            return Task.FromResult( string.Empty );
        }

    }
}