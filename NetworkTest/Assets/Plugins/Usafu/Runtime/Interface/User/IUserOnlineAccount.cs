using System.Threading.Tasks;

namespace FishingCactus.User
{
    public interface IAccessTokenRequestInfos
    {}

    public interface IUserOnlineAccount : IOnlineUser
    {
        Task< string > GetAccessToken( IAccessTokenRequestInfos request_infos = null );
        bool GetAuthAttributeByName( out string attribute_value, string attribute_name );
        bool SetUserAttributeByName( string attribute_value, string attribute_name );
    }
}