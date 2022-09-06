using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using FishingCactus.Setup;
using FishingCactus.User;
using FishingCactus.Util;

namespace FishingCactus.SaveGameSystem
{
   public abstract class SaveGameSystemBase : ISaveGameSystem
    {
        public virtual void Initialize( Settings settings )
        {
            this.settings = settings;
        }

        public abstract Task<bool> DeleteGameAsync( string name, IUniqueUserId user_index );

        public virtual async Task<bool> DoesSaveGameExistAsync( string name, IUniqueUserId user_index )
        {
            return await DoesSaveGameExistWithResultAsync( name, user_index ) == ESaveExistsResult.Ok;
        }

        public abstract Task<ESaveExistsResult> DoesSaveGameExistWithResultAsync( string name, IUniqueUserId user_index );
        public virtual Task<Tuple<bool, ReadOnlyCollection<string>>> FindFilesAsync( IUniqueUserId user_index, string search_pattern = null )
        {
            ReadOnlyCollection< string > found_files;
            try
            {
                if( string.IsNullOrEmpty( search_pattern ) )
                {
                    search_pattern = "*.*";
                }

                var entries = Directory.GetFiles( USAFUCore.Get().Platform.PersistentDataPath );
                var matches = new List<string> ( (int) entries.Length );

                for ( var i = 0; i < entries.Length; ++i )
                {
                    var entry = entries[i];
                    var entry_name = Path.GetFileNameWithoutExtension( entry );
                    
                    if ( entry_name != "." && entry_name != ".." )
                    {
                        if ( entry.MatchWildcard( search_pattern ))
                        {
                            matches.Add( entry_name );
                        }
                    }
                }

                found_files = matches.AsReadOnly();
            }
            catch( Exception e )
            {
                Logger.Log( LogLevel.Warning, "Failed to find files: " + e.Message );
                found_files = new ReadOnlyCollection< string >( new List< string >() );
                return Task.FromResult( Tuple.Create( false, found_files ) );
            }

            return Task.FromResult( Tuple.Create( true, found_files ) );
        }

        public virtual string GetSaveFileNameWithExtension( string name )
        {
            return $"{name}{settings.SaveFileExtensionWithDot}";
        }

        public abstract Task<bool> LoadGameAsync( string name, IUniqueUserId user_index, MemoryStream out_data );
        public abstract Task<bool> SaveGameAsync( string name, IUniqueUserId user_index, MemoryStream data, SaveSettings settings = null );

#if USAFU_SAVE_ALLOW_WAIT
        private bool isSavingOrLoadingGame = false;
#endif
        public async Task<bool> SaveGameAsync<T>( string name, IUniqueUserId user_index, T data, SaveSettings settings = null )
        {
#if USAFU_SAVE_ALLOW_WAIT
            while ( isSavingOrLoadingGame )
            {
                await Task.Delay( 10 );
            }

            isSavingOrLoadingGame = true;
#endif
            var result = false;

            using ( var stream = new MemoryStream() )
            {
                try
                {
                    ( new BinaryFormatter() ).Serialize( stream, data );
                    result = await SaveGameAsync( name, user_index, stream, settings );
                }
                catch ( System.Exception e )
                {
                    Logger.Log( LogLevel.Error, e.Message );
                }
            }

#if USAFU_SAVE_ALLOW_WAIT
            isSavingOrLoadingGame = false;
#endif
            return result;
        }

        public async Task<Tuple<bool, T>> LoadGameAsync<T>( string name, IUniqueUserId user_index )
        {
#if USAFU_SAVE_ALLOW_WAIT
            while ( isSavingOrLoadingGame )
            {
                await Task.Delay( 10 );
            }

            isSavingOrLoadingGame = true;
#endif

            using ( var stream = new MemoryStream() )
            {
                T data;
                var result = await LoadGameAsync( name, user_index, stream );
                if ( result )
                {
                    data = ( T ) ( new BinaryFormatter() ).Deserialize( stream );
                }
                else
                {
                    data = default;
                }

#if USAFU_SAVE_ALLOW_WAIT
                isSavingOrLoadingGame = false;
#endif

                return Tuple.Create( result, data );
            }
        }

        protected Settings settings;
    }
}
