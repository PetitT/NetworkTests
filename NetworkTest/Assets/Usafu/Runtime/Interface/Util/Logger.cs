namespace FishingCactus.Util
{
    public enum LogLevel
    {
        Verbose,
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public static class Logger
    {
        public delegate void OnLogDelegate( LogLevel log_level, string message );

        public static event OnLogDelegate OnLog;

        public static void Log( LogLevel level, string message )
        {
            OnLog?.Invoke( level, message );
        }
    }
}