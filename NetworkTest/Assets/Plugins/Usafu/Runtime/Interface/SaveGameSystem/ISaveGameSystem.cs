using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using FishingCactus.User;

namespace FishingCactus.SaveGameSystem
{
    public interface ISaveGameSystem
    {
        void Initialize( Setup.Settings settings );
        Task<bool> DoesSaveGameExistAsync( string name, IUniqueUserId user_index );
        Task<ESaveExistsResult> DoesSaveGameExistWithResultAsync( string name, IUniqueUserId user_index );
        Task<bool> SaveGameAsync( string name, IUniqueUserId user_index, MemoryStream data, SaveSettings settings = null );
        Task<bool> SaveGameAsync<T>( string name, IUniqueUserId user_index, T data, SaveSettings settings = null );
        Task<bool> LoadGameAsync( string name, IUniqueUserId user_index, MemoryStream out_data );
        Task<Tuple<bool, T>> LoadGameAsync<T>( string name, IUniqueUserId user_index );
        Task<bool> DeleteGameAsync( string name, IUniqueUserId user_index );
        Task<Tuple<bool, ReadOnlyCollection<string>>> FindFilesAsync( IUniqueUserId user_index, string search_pattern = null );
        string GetSaveFileNameWithExtension( string name );
    }

    public enum ESaveExistsResult
    {
        Ok,
        DoesNotExist,
        Corrupt,
        UnspecifiedError
    }
}