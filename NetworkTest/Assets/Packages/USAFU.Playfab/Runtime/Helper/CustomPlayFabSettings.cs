using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishingCactus.PlayFabIntegration
{
    [CreateAssetMenu(fileName = "PlayFabSharedSettings", menuName = "PlayFab/CustomSharedSettings")]
    public class CustomPlayFabSettings : PlayFabSharedSettings
    {
        //TYPES
        public enum BuildType
        {
            CLIENT,
            LOCAL_SERVER,
            REMOTE_SERVER
        }

        //FIELDS
        public BuildType Type;
        public string BuildId = "";
        public bool EnableLogs = false;
        public string[] PreferredRegions;
    }
}
