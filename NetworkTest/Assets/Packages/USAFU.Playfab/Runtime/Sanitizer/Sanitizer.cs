using FishingCactus.User;
using System.Threading.Tasks;

namespace FishingCactus.Sanitizer
{
    public class Sanitizer : ISanitizer
    {
        public Task< SanitizeMessageResult > SanitizeMessage( IUniqueUserId user_id, string message )
        {
            return Task.FromResult( new SanitizeMessageResult { Success = true, Message = message }  );
        }
    }
}