using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FishingCactus.Util.Logger;
using static FishingCactus.Util.HelperMethods;

namespace FishingCactus.User
{
    public class OnlineUserSystem : IOnlineUserSystem
    {
        public Task<bool> QueryUserInfo( IUniqueUserId user_id, List<IUniqueUserId> queried_user_ids )
        {
            if( !IsUserValid( user_id ) )
            {
                return Task.FromResult( false );
            }

            Log( Util.LogLevel.Info, "Querrying user info" );
            var task_completion_source = new TaskCompletionSource<bool>();
            var request = new GetUserDataRequest { };

            PlayFabClientAPI.GetUserData(
                request,
                ( result ) =>
                {
                    Log( Util.LogLevel.Info, "Found user info" );

                    foreach( var data in result.Data )
                    {
                        USAFUCore.Get().UserSystem.GetUserAccount(
                           USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ))
                        .SetUserAttributeByName( data.Value.Value, data.Key );
                    }

                    task_completion_source.TrySetResult( true );
                },
                ( error ) =>
                {
                    Log( Util.LogLevel.Error, $"Couldn't find user info : {error.GenerateErrorReport()}" );
                    task_completion_source.TrySetResult( false );
                } );

            return task_completion_source.Task;
        }

        public IEnumerable<IOnlineUser> GetAllUserInfo( IUniqueUserId user_id )
        {
            return Enumerable.Empty<IOnlineUser>();
        }

        public IOnlineUser GetUserInfo( IUniqueUserId user_id, IUniqueUserId queried_user_id )
        {
            return null;
        }
    }
}