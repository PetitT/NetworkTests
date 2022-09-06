using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishingCactus.Platform
{
    public delegate void OnSuspendApplicationDelegate();
    public delegate void OnResumeApplicationDelegate( double seconds_suspended );
    public delegate void OnNetworkStatusChangedDelegate( EOnlineServerConnectionStatus status );

    public interface IFile
    {
        bool Exists( string file_path );
        bool WriteAllText( string file_path, string contents );
        bool ReadAllText( string file_path, out string contents );
        bool Delete( string file_path );
    }

    public interface IDirectory
    {
        bool Exists( string directory_path );
        bool Create( string directory_path );
        bool Delete( string directory_path );
        IEnumerable< string > GetFiles( string parent_directory_path );
        IEnumerable< string > GetDirectories( string parent_directory_path );
    }

    public interface IIO
    {
        IFile File { get; }
        IDirectory Directory { get; }
    }

    public enum EOnlineServerConnectionStatus
    {
        Normal,
        ServiceUnavailable
    }

    public interface IPlatform
    {
        string PersistentDataPath { get; }
        string CacheDataPath { get; }
        string PlatformName { get; }
        bool IsInitialized {  get; }
        bool IsConsole { get; }
        string ApplicationId { get; }
        int MaxLocalPlayers { get; }
        IIO IO { get; }
        bool IsNetworkAvailable { get; }
        bool IsInBackground { get; }

        event OnSuspendApplicationDelegate OnSuspendApplication;
        event OnResumeApplicationDelegate OnResumeApplication;
        event OnNetworkStatusChangedDelegate OnNetworkStatusChanged;

        void Initialize(Setup.Settings platform_settings);
        void Dispose();
    }

    public abstract class PlatformBase : IPlatform
    {
        public event OnSuspendApplicationDelegate OnSuspendApplication;
        public event OnResumeApplicationDelegate OnResumeApplication;
        public event OnNetworkStatusChangedDelegate OnNetworkStatusChanged;

        public string PersistentDataPath { get; private set; }
        public string CacheDataPath { get; private set; }
        public abstract string PlatformName { get; }
        public abstract bool IsInitialized {  get; }
        public abstract bool IsConsole { get; }
        public abstract string ApplicationId { get; }
        public abstract int MaxLocalPlayers { get; }
        public IIO IO { get; private set; }
        public abstract bool IsInBackground { get; }

        public virtual bool IsNetworkAvailable { get; protected set; } = true;

        public virtual void Initialize( Setup.Settings platform_settings )
        {
            PersistentDataPath = GetPersistentDataFile();
            CacheDataPath = GetCacheDataPath();
            IO = GetIO();
        }

        public virtual void Dispose()
        {}

        protected void InvokeOnSuspendApplication()
        {
            OnSuspendApplication?.Invoke();
        }

        protected void InvokeOnResumeApplication( double seconds_suspended )
        {
            OnResumeApplication?.Invoke( seconds_suspended );
        }

        protected void InvokeOnNetworkStatusChanged( EOnlineServerConnectionStatus status )
        {
            OnNetworkStatusChanged?.Invoke( status );
        }

        protected virtual IIO GetIO()
        {
            return new MonoIO();
        }

        protected virtual string GetPersistentDataFile()
        {
            return Application.persistentDataPath;
        }

        protected virtual string GetCacheDataPath()
        {
            return Application.persistentDataPath;
        }
    }
}