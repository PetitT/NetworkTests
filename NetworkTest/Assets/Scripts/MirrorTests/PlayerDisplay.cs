using Mirror;
using PlayFabIntegration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerDisplay : NetworkBehaviour
{
    [SyncVar(hook = nameof(DisplayName))] string playerName;
    [SyncVar(hook = nameof(DisplayColor))] string teamName;
    TMP_Text text;
    MeshRenderer bodyMesh;


    private void DisplayName(string oldValue, string newValue)
    {
        //Debug.Log($"Updated name from -{oldValue}- to -{newValue}-");
        if (text == null)
        {
            text = GetComponentInChildren<TMP_Text>();
        }
        text.text = playerName;
    }

    private void DisplayColor(string oldValue, string newValue)
    {
        //Debug.Log($"Updated color from -{oldValue}- to -{newValue}-");
        if (string.IsNullOrEmpty(newValue)) { return; }

        if (bodyMesh == null)
        {
            bodyMesh = GetComponent<MeshRenderer>();
        }
        bodyMesh.material.color = newValue == "Blue" ? Color.blue : Color.red;
    }

    private void Start()
    {
        text = GetComponent<TMP_Text>();
        string displayName = PlayFabManager.Instance.DisplayName;

        string teamId = "";

        try
        {
            teamId = PlayFabManager.Instance.MatchmakingManager.MatchResult.Members.FirstOrDefault(
               t => t.Entity.Id == PlayFabManager.Instance.EntityID)
               .TeamId;
        }
        catch
        {
            //Debug.Log("Player was not part of a team");
        }

        //Debug.Log($"My name is {displayName} and my teamId is {teamId}");
        CmdSetDatas(displayName, teamId);
    }

    [Command]
    private void CmdSetDatas(string newName, string teamID)
    {
        playerName = newName;
        teamName = teamID;
    }
}
