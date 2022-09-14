using PlayFab;
using UnityEngine;

namespace FishingCactus.PlayFabIntegration
{
    public static class PlayFabLogging
    {
        static bool shouldLog => PlayFabManager.Instance.Configuration.playFabDebugging; 

        public static void LogError(
            string text, 
            PlayFabError error
            )
        {
            if ( shouldLog )
            {
                Debug.LogError( $"{ text } : { error.GenerateErrorReport() }" );
            }
        }

        public static void Log( string text )
        {
            if ( shouldLog )
            {
                Debug.Log( text );
            }
        }

        public static void LogWarning( string text )
        {
            if( shouldLog )
            {
                Debug.LogWarning( text );
            }
        }
    }
}
