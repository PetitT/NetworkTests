using System.Collections.Generic;
using System.Threading.Tasks;
using FishingCactus.User;

namespace FishingCactus.AddOns
{
    public interface IAddOnSystem
    {
        void Initialize( Setup.Settings settings );
        Task<List<string>> GetAddOns( IUniqueUserId user_id );
    }

    public abstract class AddOnSystemBase : IAddOnSystem
    {
        public virtual void Initialize( Setup.Settings settings )
        {
        }

        public abstract Task<List<string>> GetAddOns(IUniqueUserId user_id);
    }
}