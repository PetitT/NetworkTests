using FishingCactus.Input;
using FishingCactus.Platform;
using FishingCactus.User;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace FishingCactus.LifeCycle
{
    public interface IApplicationLifeCycle
    {
        event OnLoginStatusChangedDelegate OnLoginStatusChanged;
        event OnControllerPairingChangedDelegate OnControllerPairingChanged;
        event OnSuspendApplicationDelegate OnSuspendApplication;
        event OnResumeApplicationDelegate OnResumeApplication;
        event OnControllerConnectionChangedDelegate OnControllerConnectionChanged;
        event OnNetworkStatusChangedDelegate OnNetworkStatusChanged;

        void AddActionToThreadDispatcher( Action action );
    }

    public class ApplicationLifeCycleBase : MonoBehaviour, IApplicationLifeCycle
    {
        public event OnLoginStatusChangedDelegate OnLoginStatusChanged;
        public event OnControllerPairingChangedDelegate OnControllerPairingChanged;
        public event OnSuspendApplicationDelegate OnSuspendApplication;
        public event OnResumeApplicationDelegate OnResumeApplication;
        public event OnControllerConnectionChangedDelegate OnControllerConnectionChanged;
        public event OnNetworkStatusChangedDelegate OnNetworkStatusChanged;

        public virtual void Awake()
        {
            DontDestroyOnLoad( this );
        }

        public virtual void Start()
        {
            USAFUCore.Get().Platform.OnResumeApplication += ( duration ) =>
            {
                AddActionToThreadDispatcher( () =>
                {
                    OnResumeApplication?.Invoke( duration );
                } );
            };

            USAFUCore.Get().Platform.OnSuspendApplication += () =>
            {
                // Don't add to threaddispatcher because we have to call XboxOnePLM.AmReadyToSuspendNow() when we're done.
                OnSuspendApplication?.Invoke();
            };

            USAFUCore.Get().UserSystem.OnControllerPairingChanged += ( int controller_id, IUniqueUserId previous_user_id, IUniqueUserId new_user_id ) =>
            {
                AddActionToThreadDispatcher( () =>
                {
                    OnControllerPairingChanged?.Invoke( controller_id, previous_user_id, new_user_id );
                } );
            };

            USAFUCore.Get().UserSystem.OnLoginStatusChanged += ( ELoginStatus old_status, ELoginStatus new_status, IUniqueUserId new_user_id ) =>
            {
                AddActionToThreadDispatcher( () =>
                {
                    OnLoginStatusChanged?.Invoke( old_status, new_status, new_user_id );
                } );
            };

            USAFUCore.Get().InputSystem.OnControllerConnectionChanged += ( bool is_connected, int controller_index ) =>
            {
                AddActionToThreadDispatcher( () =>
                {
                    OnControllerConnectionChanged?.Invoke( is_connected, controller_index );
                } );
            };

            USAFUCore.Get().Platform.OnNetworkStatusChanged += ( EOnlineServerConnectionStatus status ) =>
            {
                AddActionToThreadDispatcher( () =>
                {
                    OnNetworkStatusChanged?.Invoke( status );
                } );
            };
        }

        public virtual void Update()
        {
            Action action;
            while ( actionQueue.TryDequeue(out action) )
            {
                action.Invoke();
            }
        }

        public void AddActionToThreadDispatcher( Action action )
        {
            if ( action != null )
            {
                actionQueue.Enqueue( action );
            }
        }

        protected virtual void OnApplicationQuit()
        {
            USAFUCore.Get().Platform.Dispose();            
        }

        private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();
    }
}
