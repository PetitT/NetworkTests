using FishingCactus.User;
using System.Threading.Tasks;

namespace FishingCactus.Sanitizer
{
    public struct SanitizeMessageResult
    {
        public bool Success;
        public string Message;
    }

    public interface ISanitizer
    {
        Task< SanitizeMessageResult > SanitizeMessage( IUniqueUserId user_id, string message );
    }
}