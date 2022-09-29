namespace FishingCactus.User
{
    public interface IOnlineUser
    {
        IUniqueUserId UserId { get; }
        string RealName { get; }
        string DisplayName{ get; }
        bool GetUserAttribute( string attribute_name, out string result );
    }
}