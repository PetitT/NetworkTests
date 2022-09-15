using PlayFab.ClientModels;
using UnityEngine;

namespace FishingCactus.PlayFabIntegration
{
    public class PlayFabManager : MonoBehaviour
    {
        //FIELDS
        private static PlayFabManager instance;
        private Configuration configuration;
        public LoginManager LoginManager { get; private set; } = new LoginManager();
        public LeaderboardManager LeaderboardManager { get; private set; } = new LeaderboardManager();
        public PlayerDataManager PlayerDataManager { get; private set; } = new PlayerDataManager();
        public TitleDataManager TitleDataManager { get; private set; } = new TitleDataManager();
        public MatchmakingManager MatchmakingManager { get; private set; } = new MatchmakingManager();
        public ServerRequestManager ServerConnectionManager { get; private set; } = new ServerRequestManager();
        public LobbyManager LobbyManager { get; private set; } = new LobbyManager();


        //PROPERTIES
        public static PlayFabManager Instance => instance ??= FindObjectOfType<PlayFabManager>();
        public Configuration Configuration => configuration ??= Resources.Load<Configuration>( "PlayfabConfiguration" );
        public bool IsLoggedIn => LoginManager.IsLoggedIn;
        public string DisplayName => LoginManager.DisplayName;
        public EntityKey EntityKey => LoginManager.EntityKey;
        public string SessionTicket => LoginManager.SessionTicket;
        public string PlayfabID => LoginManager.PlayFabID;

        //UNITY
        private void Start()
        {
            DontDestroyOnLoad( this );
        }

        private void Update()
        {
            if ( IsLoggedIn )
            {
                MatchmakingManager.Tick( Time.deltaTime );
            }
        }

        private void OnDestroy()
        {
            if ( IsLoggedIn )
            {
                MatchmakingManager.CancelAllMatchmakingQueuesForUser(); //We need to manually cancel matchmaking because we are still in queue after disconnecting
                LobbyManager.LeaveCurrentLobby();
            }
        }
    }
}
