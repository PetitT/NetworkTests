using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OculusIntegration
{
    public class UsersManager
    {
        private event Action<User> onGotLoggedInUser;

        public ulong UserID { get; private set; }
        public string UserName { get; private set; }

        public void InitializeLoggedInUserInfo(Action<User> onResult = null)
        {
            onGotLoggedInUser = onResult;
            Users.GetLoggedInUser().OnComplete(OnGotLoggedInUser); //Only returns the UserID, we need to call Users.Get to have all info  
        }

        private void OnGotLoggedInUser(Message<User> message)
        {
            if (message.IsError)
            {
                Debug.Log("Couldn't find logged in user");
                onGotLoggedInUser?.Invoke(null);
                return;
            }

            UserID = message.Data.ID;
            Debug.Log($"Found user : ID is {UserID}");
            Users.Get(UserID).OnComplete(OnGotSelfUser); 
        }

        private void OnGotSelfUser(Message<User> message)
        {
            if (message.IsError)
            {
                Debug.Log("Couldn't find user");
                onGotLoggedInUser?.Invoke(null);
                return;
            }

            UserName = message.Data.DisplayName;
            onGotLoggedInUser?.Invoke(message.Data);
        }
    }
}
