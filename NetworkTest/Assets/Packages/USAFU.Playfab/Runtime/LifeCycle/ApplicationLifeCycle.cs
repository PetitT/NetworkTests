using PlayFab.Multiplayer;

namespace FishingCactus.LifeCycle
{
    public class ApplicationLifeCycle : ApplicationLifeCycleBase
    {
        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            base.Update();
            if( USAFUCore.Get().Platform.IsInitialized )
            {
                PlayFabMultiplayer.ProcessLobbyStateChanges();
                PlayFabMultiplayer.ProcessMatchmakingStateChanges();
            }
        }
    }
}