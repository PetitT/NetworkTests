using Mirror;
using PlayFabIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameDisplay : NetworkBehaviour
{
    [SyncVar(hook = nameof(DisplayName))] string playerName;
    TMP_Text text;


    private void DisplayName(string oldValue, string newValue)
    {
        Debug.Log($"Updated name from -{oldValue}- to -{newValue}-");
        if (text == null)
        {
            text = GetComponentInChildren<TMP_Text>();
        }
        text.text = playerName;
    }

    private void Start()
    {
        text = GetComponent<TMP_Text>();
        string displayName = PlayFabManager.Instance.DisplayName;
        Debug.Log($"My name is {displayName}");
        CmdSetName(displayName);
    }

    [Command]
    private void CmdSetName(string newName)
    {
        playerName = newName;
    }
}
