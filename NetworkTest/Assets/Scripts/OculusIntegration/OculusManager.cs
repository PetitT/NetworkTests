using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OculusIntegration
{
    public class OculusManager : MonoBehaviour
    {
        private static OculusManager instance;
        public static OculusManager Instance => instance = instance != null ? instance : FindObjectOfType<OculusManager>();

        public StartupManager StartupManager { get; private set; } = new StartupManager();
        public GroupPresenceManager GroupPresenceManager { get; private set; } = new GroupPresenceManager();
        public UsersManager UsersManager { get; private set; } = new UsersManager();

        public bool Initialized => StartupManager.Initialized;
        public string UserName => StartupManager.Username;
        public ulong UserID => UsersManager.UserID;
    }
}
