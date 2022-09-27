using UnityEngine;
using UnityEditor;

namespace FishingCactus.PlayFabIntegration.Editor
{
    public static class AutoCreateConfigFiles
    {
        [InitializeOnLoadMethod]
        private static void CheckCustomSharedSettingsExistence()
        {
            if( !AssetDatabase.IsValidFolder( "Assets/Resources" ) )
            {
                AssetDatabase.CreateFolder( "Assets", "Resources" );
            }

            if( Resources.Load<CustomPlayFabSettings>( "PlayFabSharedSettings" ) == null )
            {
                CustomPlayFabSettings new_object = ScriptableObject.CreateInstance<CustomPlayFabSettings>();
                AssetDatabase.CreateAsset( new_object, "Assets/Resources/PlayFabSharedSettings.asset" );
                AssetDatabase.SaveAssets();
            }
        }
    }
}
