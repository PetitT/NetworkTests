using FishingCactus;
using FishingCactus.Unity;
using FishingCactus.User;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UsafuTest : MonoBehaviour
{
    private async void Start()
    {
        while (!USAFUCore.Get().Platform.IsInitialized)
        {
           await Task.Yield();
        }

        Debug.Log("Begin Login");
        LoginResult result = await USAFUCore.Get().UserSystem.Login(0);
        Debug.Log($"result is {result.UserId}");
    }
}
