using FishingCactus.Setup;
using FishingCactus.Unity;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class USAFUEditor
{
    [MenuItem( "Assets/USAFU/AchievementsList" )]
    public static void CreateAchievementsList()
    {
        CreateScriptableObject< AchievementMappingList >();
    }

    [MenuItem( "Assets/USAFU/PlatformSetup" )]
    public static void CreatePlatformSetup()
    {
        CreateScriptableObject< Settings >();
    }

    private static void CreateScriptableObject< T >()
        where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof( T ).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
