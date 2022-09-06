using FishingCactus.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishingCactus.SaveGameSystem
{
    [Serializable]
    public class PlayerPrefsSaveFile
    {
        [field: NonSerialized]
        public IUniqueUserId UserId { get; private set; }
        [field: NonSerialized]
        public string FileName { get; private set; }
        [field: NonSerialized]
        public SaveSettings SaveSettings { get; private set; }

        [SerializeField]
        private Dictionary<string, float> floatValues = new Dictionary<string, float>();
        [SerializeField]
        private Dictionary<string, int> integerValues = new Dictionary<string, int>();
        [SerializeField]
        private Dictionary<string, string> stringValues = new Dictionary<string, string>();

#if USAFU_SAVE_FORWARD_PLAYER_PREFS
        [field: NonSerialized]
        public Func<string, string> KeyGenerator { get; set; }
#else
        [field: NonSerialized]
        private bool IsDirty = false;
#endif

        public void Initialize( IUniqueUserId user_id, string file_name, SaveSettings save_settings = null )
        {
            UserId = user_id;
            FileName = file_name;
            SaveSettings = save_settings;
        }

        public int GetInt( string key, int default_value )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            return PlayerPrefs.GetInt( GetKey( key ), default_value );
#else
            int result;
            return integerValues.TryGetValue( key, out result ) ? result : default_value;
#endif
        }

        public int GetInt( string key )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            return PlayerPrefs.GetInt( GetKey( key ) );
#else
            return integerValues [ key ];
#endif
        }

        public void SetInt( string key, int value )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            PlayerPrefs.SetInt( GetKey( key ), value );
#else
            TrySetValueIfDifferent( key, value, integerValues );
#endif
        }

        public float GetFloat( string key, float default_value )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            return PlayerPrefs.GetFloat( GetKey( key ), default_value );
#else
            return floatValues.TryGetValue( key, out float result ) ? result : default_value;
#endif
        }

        public float GetFloat( string key )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            return PlayerPrefs.GetFloat( GetKey( key ) );
#else
            return floatValues [ key ];
#endif
        }

        public void SetFloat( string key, float value )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            PlayerPrefs.SetFloat( GetKey( key ), value );
#else
            TrySetValueIfDifferent( key, value, floatValues );
#endif
        }

        public string GetString( string key, string default_value )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            return PlayerPrefs.GetString( GetKey( key ), default_value );
#else
            string result;
            stringValues.TryGetValue( key, out result );
            return result ?? default_value;
#endif
        }

        public string GetString( string key )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            return PlayerPrefs.GetString( GetKey( key ) );
#else
            return GetString( key, string.Empty );
#endif
        }

        public void SetString( string key, string value )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            PlayerPrefs.SetString( GetKey( key ), value );
#else
            TrySetValueIfDifferent( key, value, stringValues );
#endif
        }

        public bool HasKey( string key )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            return PlayerPrefs.HasKey( GetKey( key ) );
#else
            return integerValues.ContainsKey( key )
                || stringValues.ContainsKey( key )
                || floatValues.ContainsKey( key );
#endif
        }

        public void DeleteKey( string key )
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            PlayerPrefs.DeleteKey( GetKey( key ) );
#else
            IsDirty &= integerValues.Remove( key );
            IsDirty &= floatValues.Remove( key );
            IsDirty &= stringValues.Remove( key );
#endif
        }

        public Task<bool> SaveGameAsync()
        {
#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            PlayerPrefs.Save();
            return Task.FromResult( true );
#else

            if ( !IsDirty )
            {
                return Task.FromResult( false );
            }

            if ( string.IsNullOrEmpty( FileName )
                || UserId == null
                )
            {
                Debug.LogError( "Impossible to save. Empty FileName or null UserId" );
                return Task.FromResult( false );
            }

            return USAFUCore.Get().SaveSystem.SaveGameAsync( FileName, UserId, this, SaveSettings )
                .ContinueWith( t =>
                {
                    IsDirty = false;
                    return t.Result;
                } );
#endif
        }

        public static async Task<PlayerPrefsSaveFile> LoadGameAsync( string file_name, IUniqueUserId user_id, SaveSettings save_settings = null )
        {
            PlayerPrefsSaveFile save_file;

#if USAFU_SAVE_FORWARD_PLAYER_PREFS
            save_file = new PlayerPrefsSaveFile();
#else
            var result = await USAFUCore.Get().SaveSystem.LoadGameAsync<PlayerPrefsSaveFile>( file_name, user_id );

            if ( result.Item1 )
            {
                save_file = result.Item2;
            }
            else
            {
                save_file = new PlayerPrefsSaveFile();
            }

            save_file.IsDirty = false;
#endif
            save_file.Initialize( user_id, file_name, save_settings );

            return save_file;
        }

#if USAFU_SAVE_FORWARD_PLAYER_PREFS
        private string GetKey( string key )
        {
            if ( KeyGenerator != null )
            {
                return KeyGenerator( key );
            }
            return key;
        }
#else
        private void TrySetValueIfDifferent< VALUE_TYPE >( string key, VALUE_TYPE value, Dictionary< string, VALUE_TYPE > dictionary )
            where VALUE_TYPE : IEquatable< VALUE_TYPE >
        {
            if ( dictionary.TryGetValue( key, out VALUE_TYPE existing_value ) )
            {
                if ( existing_value != null && existing_value.Equals( value ) )
                {
                    return;
                }
            }

            dictionary[ key ] = value;
            IsDirty = true;
        }
#endif
    }

}