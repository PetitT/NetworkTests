using PlayFab.ClientModels;
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
        private static PlayFabManager instance;
        public static PlayFabManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PlayFabManager>();
                }
                return instance;
            }
        }

        public bool IsLoggedIn => LoginManager.IsLoggedIn;
        public string DisplayName => LoginManager.DisplayName;
        public string PlayfabID => LoginManager.LoggedInPlayFabID;
        public string EntityID => LoginManager.EntityID;
        public EntityKey EntityKey => LoginManager.EntityKey;
        public string SessionTicket => LoginManager.SessionTicket;

        public LoginManager LoginManager { get; private set; } = new LoginManager();
        public LeaderboardManager LeaderboardManager { get; private set; } = new LeaderboardManager();
        public PlayerDataManager PlayerDataManager { get; private set; } = new PlayerDataManager();
        public TitleDataManager TitleDataManager { get; private set; } = new TitleDataManager();
        public MatchmakingManager MatchmakingManager { get; private set; } = new MatchmakingManager();
        public ServerConnectionManager ServerConnectionManager { get; private set; } = new ServerConnectionManager();
        public LobbyManager LobbyManager { get; private set; } = new LobbyManager();

        private void Update()
        {
            if (IsLoggedIn)
            {
                MatchmakingManager.Tick(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (IsLoggedIn)
            {
                MatchmakingManager.CancelAllMatchmakingQueuesForUser(); //We need to manually cancel matchmaking because we are still in queue after disconnecting
                LobbyManager.LeaveCurrentLobby();
            }
        }
    }
}
