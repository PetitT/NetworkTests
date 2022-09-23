using FishingCactus.User;
using System.Threading.Tasks;

namespace FishingCactus.ExternalUI
{
    public class ExternalUI : ExternalUIBase
    {
        public override Task<ShowLoginUiResult> ShowLoginUi( int controller_id, bool show_skip_button )
        {
            return Task.FromResult( new ShowLoginUiResult() );
        }

        public override Task<bool> ShowInviteUI( IUniqueUserId user_id, string session_name )
        {
            if( user_id == null
                || !user_id.IsValid
                )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Failed to show the invite UI : Invalid user id" );
                return Task.FromResult( false );
            }

            if( USAFUCore.Get().OnlineSessions == null )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Failed to show the invite UI : Online sessions are not enabled" );
                return Task.FromResult( false );
            }

            if(USAFUCore.Get().OnlineSessions.GetNamedSession(session_name) != null )
            {
                Util.Logger.Log( Util.LogLevel.Error, "Failed to show the invite UI : Not in a session" );
                return Task.FromResult( false );
            }

            Util.Logger.Log( Util.LogLevel.Info, "Displaying Invite UI" );
            var options = new Oculus.Platform.InviteOptions();
            Oculus.Platform.GroupPresence.LaunchInvitePanel( options );
            return Task.FromResult( true );
        }
    }
}