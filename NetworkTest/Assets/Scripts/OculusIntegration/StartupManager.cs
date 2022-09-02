using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OculusIntegration
{
    public class StartupManager
    {
        public bool Initialized { get; private set; }
        public string Username { get; private set; }

        public void AsyncInitializeCore(Message<PlatformInitialize>.Callback callback = null)
        {
            Core.AsyncInitialize().OnComplete(callback);
            Initialized = true;
        }

        public void CheckEntitlement(Action<bool> isEntitledCallback = null)
        {
            Entitlements.IsUserEntitledToApplication().OnComplete(
                message =>
                {
                    if (message.IsError)
                    {
                        Debug.Log("You are not entitled to the app...");
                        isEntitledCallback.Invoke(false);
                    }
                    else
                    {
                        Debug.Log("You are entitled to the app !");
                        isEntitledCallback.Invoke(true);
                    }
                });
        }
    }
}
