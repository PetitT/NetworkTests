using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayFabIntegration
{
    public static class PlayFabLogging
    {
        public static void LogError(string text, PlayFabError error)
        {
            Debug.LogError($"{text} : {error.GenerateErrorReport()}");
        }

        public static void Log(string text)
        {
            Debug.Log(text);
        }
    }
}
