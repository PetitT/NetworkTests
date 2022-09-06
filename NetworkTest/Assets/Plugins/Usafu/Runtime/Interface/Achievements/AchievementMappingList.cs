using System;
using FishingCactus.OnlineAchievements;
using System.Collections.Generic;
using UnityEngine;

namespace FishingCactus.Unity
{
    [Serializable]
    public class AchievementMappingList : ScriptableObject
    {
        public List<AchievementMapping> AchievementMappings = new List<AchievementMapping>();
    }
}