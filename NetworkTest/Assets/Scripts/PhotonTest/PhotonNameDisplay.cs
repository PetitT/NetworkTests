using Fusion;
using OculusIntegration;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhotonNameDisplay : NetworkBehaviour
{
    public TMP_Text nameText;
    [Networked(OnChanged = nameof(OnNameChanged))]
    string playerName { get; set; }

    private static void OnNameChanged(Changed<PhotonNameDisplay> change)
    {
        change.Behaviour.nameText.text = change.Behaviour.playerName;
    }

    private void Start()
    {
        if (Object.HasInputAuthority)
        {
            string name = FindObjectOfType<OculusPhotonPresence>().userName;
            RPC_SetName(name);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_SetName(string name, RpcInfo info = default)
    {
        playerName = name;
    }
}
