using System;
using System.Linq;
using System.Reflection;
using FishingCactus.AddOns;
using FishingCactus.Setup;
using FishingCactus.User;
using FishingCactus.Input;
using FishingCactus.Util;
using FishingCactus.LifeCycle;
using UnityEngine;
using Logger = FishingCactus.Util.Logger;

namespace FishingCactus.Unity
{
    public class USAFU
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            var core = USAFUCore.Get();

            var settings_file_name = $"USAFU/USAFUSettings";
            var settings = Resources.Load<Settings>( settings_file_name );

            if ( settings == null )
            {
                throw new SystemException( $"Impossible to load the USAFU settings file '{settings_file_name}' in the Resources folder" );
            }

            core.Settings = settings;

            Logger.OnLog += Log;

            // :NOTE: Use reflection to instantiate all systems.
            // This is required because the implementation of the class depends on the platform.
            // Due to assembly dependencies, this seems to be the easiest way to do what we want.

            // First, figure out which USAFU implementation is active.
            var loaded_usafu_assembly = GetLoadedUSAFUImplementationAssemblyName();
            core.Platform = CreateModule<Platform.IPlatform>(loaded_usafu_assembly, "FishingCactus.Platform.Platform"); ;
            core.ExternalUI = CreateModule<ExternalUI.IExternalUI>(loaded_usafu_assembly, "FishingCactus.ExternalUI.ExternalUI");
            core.UserSystem = CreateModule<User.IPlatformUserSystem>(loaded_usafu_assembly, "FishingCactus.User.PlatformUserSystem");
            core.UserPresence = CreateModule<UserPresence.IUserPresence>(loaded_usafu_assembly, "FishingCactus.UserPresence.UserPresence");

            core.AddOnSystem = CreateModule<AddOns.IAddOnSystem>(loaded_usafu_assembly, "FishingCactus.AddOns.AddOnSystem");

            core.OnlineUserSystem = CreateModule<User.IOnlineUserSystem>(loaded_usafu_assembly, "FishingCactus.User.OnlineUserSystem");

            // This is defined in the interface assembly
            var input_system = new InputSystem();
            core.InputSystem = input_system;

            core.PlatformInput = CreateModule<Input.IPlatformInput>(loaded_usafu_assembly, "FishingCactus.Input.PlatformInput");

            core.Sanitizer = CreateModule<Sanitizer.ISanitizer>(loaded_usafu_assembly, "FishingCactus.Sanitizer.Sanitizer");

            core.Friends = CreateModule<OnlineFriends.IOnlineFriends>(loaded_usafu_assembly, "FishingCactus.OnlineFriends.OnlineFriends");

            core.SocialPermissions = CreateModule<SocialPermissions.ISocialPermissions>(loaded_usafu_assembly, "FishingCactus.SocialPermissions.SocialPermissions");

            core.Statistics = CreateModule<OnlineStatistics.IOnlineStatistics>(loaded_usafu_assembly, "FishingCactus.OnlineStatistics.OnlineStatistics");

            if ( settings.UseOnlineSessions )
            {
                core.OnlineSessions = CreateModule<OnlineSessions.IOnlineSessions>(loaded_usafu_assembly, "FishingCactus.OnlineSessions.OnlineSessions");
            }

            if ( settings.UseAchievements )
            {
                core.Achievements = CreateModule<OnlineAchievements.IOnlineAchievements>(loaded_usafu_assembly, "FishingCactus.OnlineAchievements.OnlineAchievements");

                var achievements_mapping_file_path = $"USAFU/USAFUAchievements_{ core.Platform.PlatformName }";
                var achievements_mapping_list = Resources.Load<AchievementMappingList>( achievements_mapping_file_path );

                if ( achievements_mapping_list == null )
                {
                    Debug.LogWarning( "No AchievementsMappingList file was loaded from the resources" );
                }
                else
                {
                    core.Achievements.Initialize( achievements_mapping_list.AchievementMappings );
                }
            }

            if ( settings.UseSaveSystem )
            {
                core.SaveSystem = CreateModule<SaveGameSystem.ISaveGameSystem>(loaded_usafu_assembly, "FishingCactus.SaveGameSystem.SaveGameSystem");
            }

            if( settings.UseOnlineLeaderboards )
            {
                core.OnlineLeaderboards = CreateModule<OnlineLeaderboards.IOnlineLeaderboards>( loaded_usafu_assembly, "FishingCactus.OnlineLeaderboards.OnlineLeaderboards" );
            }

            
            // Just to get it created
            var game_object = new GameObject();
            var application_lifecycle_type = loaded_usafu_assembly.GetType("FishingCactus.LifeCycle.ApplicationLifeCycle");
            var life_cycle = game_object.AddComponent(application_lifecycle_type) as IApplicationLifeCycle;
            game_object.name = "ApplicationLifeCycle";
            GameObject.DontDestroyOnLoad( game_object );
            
            core.ApplicationLifeCycle = life_cycle;
        
            core.Platform.Initialize( settings );
            core.UserSystem.Initialize( settings );
            core.InputSystem.Initialize( settings );
            core.SaveSystem?.Initialize( settings );
            core.PlatformInput.Initialize( settings );
            core.AddOnSystem.Initialize( settings );
            core.OnlineSessions?.Initialize( settings );
            core.SocialPermissions.Initialize( settings );
            core.OnlineLeaderboards?.Initialize( settings );
        }

        private static void Log( LogLevel log_level, string message )
        {
            switch( log_level )
            {
                case LogLevel.Verbose:
                {
                    Debug.unityLogger.Log( "[VERBOSE]", message );
                }
                break;
                case LogLevel.Debug:
                {
                    Debug.unityLogger.Log( "[DEBUG]", message );
                }
                break;
                case LogLevel.Info:
                {
                    Debug.unityLogger.Log( "[INFO]", message );
                }
                break;
                case LogLevel.Warning:
                {
                    Debug.unityLogger.LogWarning( "[WARNING]", message );
                }
                break;
                case LogLevel.Error:
                {
                    Debug.unityLogger.LogError( "[ERROR]", message );
                }
                break;
                case LogLevel.Fatal:
                {
                    Debug.unityLogger.LogError( "[FATAL]", message );
                    Application.Quit();
                }
                break;
                default:
                    break;
            }
        }

        private static Assembly GetLoadedUSAFUImplementationAssemblyName()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var filtered_assemblies = assemblies
                .Where(assembly =>
                {
                    var name_part = assembly.FullName.Split(',')[0];
                    return name_part.StartsWith("FishingCactus.USAFU.PlayFab") && !name_part.EndsWith("Editor");
                } );
            Debug.Assert(filtered_assemblies.Count() == 1);
            return filtered_assemblies.ElementAt(0);
        }

        private static T CreateModule<T>(Assembly assembly, string class_name) where T : class
        {
            Type resolved_type = assembly.GetType(class_name);
            return Activator.CreateInstance(resolved_type) as T;
        }
    }
}
