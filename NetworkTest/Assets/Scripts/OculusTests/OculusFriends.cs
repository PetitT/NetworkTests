using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;

public class OculusFriends : MonoBehaviour
{
    void Start()
    {
        Request r = Core.AsyncInitialize();
        r.OnComplete(OnInitialize);
    }

    private void OnInitialize(Message message)
    {
        var user = Users.GetLoggedInUser();
        user.OnComplete(OnGotUser);
    }

    private void OnGotUser(Message<User> message)
    {
        Debug.Log(message.Data.DisplayName);
    }
}
