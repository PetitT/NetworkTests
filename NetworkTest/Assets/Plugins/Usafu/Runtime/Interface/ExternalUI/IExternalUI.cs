using System;
using System.Threading.Tasks;
using FishingCactus.User;
using UnityEngine;

namespace FishingCactus.ExternalUI
{
    public struct ShowLoginUiResult
    {
        public bool Success;
        public IUniqueUserId UserId;
        public int ControllerId;
    }

    public struct ShowStoreParams
    {
        public string Category;
        public string ProductId;
    }

    public interface IExternalUI
    {
        Task< ShowLoginUiResult > ShowLoginUi( int controller_id, bool show_skip_button );
        Task< bool > ShowInviteUI( IUniqueUserId user_id, string session_name );
        Task< bool > ShowWebUrl( IUniqueUserId user_id, string url );
        Task< bool > ShowStoreUI( IUniqueUserId user_id, ShowStoreParams show_store_params );
        Task< bool > ShowAccountUpgradeUI( IUniqueUserId user_id );
    }

    public abstract class ExternalUIBase : IExternalUI
    {
        public virtual Task<bool> ShowInviteUI( IUniqueUserId user_id, string session_name )
        {
            return Task.FromResult( false );
        }

        public abstract Task<ShowLoginUiResult> ShowLoginUi( int controller_id, bool show_skip_button );

        public virtual Task<bool> ShowStoreUI( IUniqueUserId user_id, ShowStoreParams show_store_params )
        {
            return Task.FromResult( false );
        }

        public virtual Task<bool> ShowWebUrl( IUniqueUserId user_id, string url )
        {
            if ( string.IsNullOrEmpty( url ) )
            { 
                return Task.FromResult( true );
            }

            Application.OpenURL( url );
            return Task.FromResult( true );
        }

        public virtual Task< bool > ShowAccountUpgradeUI( IUniqueUserId user_id )
        {
            return Task.FromResult( false );
        }
    }
}