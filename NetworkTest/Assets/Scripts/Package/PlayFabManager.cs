using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayFabIntegration
{
    /// <summary>
    /// The playfab manager serves as a parent class containing all the features of the playfab integration
    /// </summary>
    public class PlayFabManager : MonoBehaviour
    {
        public bool IsLoggedIn => LoginManager.IsLoggedIn;
        public string DisplayName => LoginManager.DisplayName;
        public string PlayfabID => LoginManager.LoggedInPlayFabID;
        public string SessionTicket => LoginManager.SessionTicket;
        public string EntityID => LoginManager.EntityID;

        public LoginManager LoginManager { get; private set; } = new LoginManager();
        public LeaderboardManager LeaderboardManager { get; private set; } = new LeaderboardManager();
        public PlayerDataManager PlayerDataManager { get; private set; } = new PlayerDataManager();
        public TitleDataManager TitleDataManager { get; private set; } = new TitleDataManager();
        public MatchmakingManager MatchmakingManager { get; private set; }

        private void Awake()
        {
            MatchmakingManager = new MatchmakingManager(this);
        }

        private void Update()
        {
            MatchmakingManager.Tick(Time.deltaTime);
        }
    }
}
