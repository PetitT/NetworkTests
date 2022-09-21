using FishingCactus.User;
using System.Threading.Tasks;

namespace FishingCactus.ExternalUI
{
    public class ExternalUI : ExternalUIBase
    {
        public override Task<ShowLoginUiResult> ShowLoginUi( int controller_id, bool show_skip_button )
        {
            return Task.FromResult( new ShowLoginUiResult
            {
                Success = true,
                UserId = new UniqueUserId(),
                ControllerId = controller_id
            } );
        }

        public override Task<bool> ShowInviteUI( IUniqueUserId user_id, string session_name )
        {
            Util.Logger.Log( Util.LogLevel.Info, "Displaying Invite UI" );
            var options = new Oculus.Platform.InviteOptions();
            Oculus.Platform.GroupPresence.LaunchInvitePanel( options );
            return Task.FromResult( true );
        }
    }
}