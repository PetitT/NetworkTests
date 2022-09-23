using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using FishingCactus.User;
using FishingCactus.Util;
using UnityEngine;
using Logger = FishingCactus.Util.Logger;

namespace FishingCactus.SaveGameSystem
{
    public class SaveGameSystem : SaveGameSystemBase
    {
        public override Task< ESaveExistsResult > DoesSaveGameExistWithResultAsync( string name, IUniqueUserId user_index )
        {
            if( File.Exists( GetSaveGamePath( name ) ) )
            {
                return Task.FromResult( ESaveExistsResult.Ok );
            }

            return Task.FromResult( ESaveExistsResult.DoesNotExist );
        }

        public override async Task< bool > SaveGameAsync( string name, IUniqueUserId user_index, MemoryStream data, SaveSettings settings = null )
        {
            try
            {
                // First, copy to a new memory stream to allow new modifications to the data.
                data.Position = 0;
                var copied_data = new MemoryStream( (int) data.Length );
                data.CopyTo( copied_data );
                copied_data.Position = 0;

                // now async save to disk
                var saved_game_path = GetSaveGamePath( name );
                var dir = Path.GetDirectoryName( saved_game_path );
                if( dir != null )
                {
                    Directory.CreateDirectory( dir );
                    using( var save_file_stream = File.Create( saved_game_path ) )
                    {
                        await copied_data.CopyToAsync( save_file_stream );
                    }
                }
            }
            catch( Exception e )
            {
                Logger.Log( LogLevel.Warning, "Failed to save file async: " + e.Message );
                return false;
            }

            return true;
        }

        public override async Task< bool > LoadGameAsync( string name, IUniqueUserId user_index, MemoryStream out_data )
        {
            try
            {
                using( var save_file_stream = File.OpenRead( GetSaveGamePath( name ) ) )
                {
                    var position = out_data.Position;
                    await save_file_stream.CopyToAsync( out_data );
                    out_data.Position = position;
                }
            }
            catch( Exception e )
            {
                Logger.Log( LogLevel.Warning, "Failed to load file async: " + e.Message );
                return false;
            }

            return true;
        }

        public override async Task< bool > DeleteGameAsync( string name, IUniqueUserId user_index )
        {
            if( ! await DoesSaveGameExistAsync( name, user_index ) )
            {
                return false;
            }

            try
            {
                File.Delete( GetSaveGamePath( name ) );
            }
            catch( Exception e )
            {
                Logger.Log( LogLevel.Warning, "Failed to delete save file: " + e.Message );
                return false;
            }

            return true;
        }

        private static string GetSaveGameLocation()
        {
            return USAFUCore.Get().Platform.PersistentDataPath;
        }

        private string GetSaveGamePath( string name )
        {
            return Path.Combine( GetSaveGameLocation(), GetSaveFileNameWithExtension( name ) );
        }
    }
}