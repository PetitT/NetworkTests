using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingCactus.User
{
    public class UserOnlineAccount : IUserOnlineAccount
    {
        public string DisplayName { get => GetDisplayName(); }
        public string RealName { get => string.Empty; }
        public IUniqueUserId UserId { get => userId; }

        private IUniqueUserId userId;

        private Dictionary<string, string> accountData = new Dictionary<string, string>();

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

        private string GetDisplayName()
        {
            if( accountData.TryGetValue( Util.StringConstants.DISPLAY_NAME, out string display_name ) )
            {
                return display_name;
            }

            Util.Logger.Log( Util.LogLevel.Warning, "User has no display name" );
            return "";
        }
    }
}