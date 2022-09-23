using System.Collections.Generic;
using System.Threading.Tasks;
using FishingCactus.User;

namespace FishingCactus.AddOns
{
    public class AddOnSystem : AddOnSystemBase
    {
        public override Task<List<string>> GetAddOns( IUniqueUserId user_id )
        {
            List<string> add_on_result = new List<string>();
            return Task.FromResult(add_on_result);
        }
    }
}