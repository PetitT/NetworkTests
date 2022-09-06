using FishingCactus;

public delegate void OnNetworkLostDelegate();
public delegate void OnNetworkAcquiredDelegate();

public static class NetworkUtilities
{
    public static event OnNetworkLostDelegate OnNetworkLost;
    public static event OnNetworkAcquiredDelegate OnNetworkAcquired;

    public static void RegisterToPlatformEvents()
    {
        if ( !USAFUCore.Get().Platform.IsConsole )
        {
            return;
        }

        USAFUCore.Get().Platform.OnResumeApplication += Platform_OnResumeApplication;
        USAFUCore.Get().Platform.OnSuspendApplication += Platform_OnSuspendApplication;
        USAFUCore.Get().Platform.OnNetworkStatusChanged += Platform_OnNetworkStatusChanged;
    }

    private static void Platform_OnNetworkStatusChanged( FishingCactus.Platform.EOnlineServerConnectionStatus status )
    {
        switch ( status )
        {
            case FishingCactus.Platform.EOnlineServerConnectionStatus.Normal:
                {
                    UnityEngine.Debug.Log( "NetworkUtilities - OnNetworkStatusChanged - Normal" );
                    InvokeOnNetworkAcquired();
                }
                break;
            case FishingCactus.Platform.EOnlineServerConnectionStatus.ServiceUnavailable:
                {
                    UnityEngine.Debug.Log( "NetworkUtilities - OnNetworkStatusChanged - ServiceUnavailable" );
                    InvokeOnNetworkLost();
                }
                break;
        }
    }

    public static bool HasNetwork
    {
        get
        {
            return USAFUCore.Get().Platform.IsNetworkAvailable;
        }
    }

    private static void InvokeOnNetworkLost()
    {
        UnityEngine.Debug.Log( "NetworkUtilities - InvokeOnNetworkLost" );

        OnNetworkLost?.Invoke();
    }

    private static void Platform_OnSuspendApplication()
    {
    }

    private static void Platform_OnResumeApplication( double seconds_suspended )
    {
    }

    public static void InvokeOnNetworkAcquired()
    {
        UnityEngine.Debug.Log( "NetworkUtilities - InvokeOnNetworkAcquired" );

        if ( !HasNetwork )
        {
            UnityEngine.Debug.LogError( $"NetworkUtilities - InvokeOnNetworkAcquired - Early Return. - !HasNetwork" );
            return;
        }

        UnityEngine.Debug.Log( "NetworkUtilities - InvokeOnNetworkAcquired - Call OnNetworkAcquired" );
        OnNetworkAcquired?.Invoke();
    }
}